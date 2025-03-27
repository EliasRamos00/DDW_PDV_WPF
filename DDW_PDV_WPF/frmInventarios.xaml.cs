using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;


namespace DDW_PDV_WPF
{



    public partial class frmInventarios : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherTimer _timer;
        private readonly ApiService _apiService;
        private ObservableCollection<ArticuloDTO> _listaArticulos;
        private ArticuloDTO _articuloSeleccionado;
   
        private string _textoBusqueda;
    

        public ObservableCollection<ArticuloDTO> ListaArticulos
        {
            get { return _listaArticulos; }
            set
            {
                _listaArticulos = value;
                OnPropertyChanged(nameof(ListaArticulos)); // Notificar cambios
            }
        }

        public ArticuloDTO ArticuloSeleccionado
        {
            get { return _articuloSeleccionado; }
            set
            {
                _articuloSeleccionado = value;
                OnPropertyChanged(nameof(ArticuloSeleccionado)); // Notificar cambios
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));
                FiltrarArticulos(); // Filtrado automático al escribir
            }
        }


        private ObservableCollection<ArticuloDTO> _todosLosArticulos; 
        public ICommand LimpiarBusquedaCommand => new RelayCommand(LimpiarBusqueda);

        private void LimpiarBusqueda()
        {
            TextoBusqueda = string.Empty;
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
            public void Execute(object parameter) => _execute();
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
        public frmInventarios()
        {
            DataContext = this;
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Actualizar cada segundo
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Inicializar ApiService
            _apiService = new ApiService();
            CargarDatos();
            DataContext = this;

            UpdateDateTime();
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrEmpty(value as string);
            if (parameter?.ToString() == "inverse") return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
        private void UpdateDateTime()
        {
            DateTime now = DateTime.Now;

            txtFecha.Text = now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"));

            txtHora.Text = now.ToString("HH:mm:ss", new CultureInfo("es-MX"));
        }

        private async void CargarDatos()
        {
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("api/CArticulos/");

            if (resultado != null)
            {
                _todosLosArticulos = new ObservableCollection<ArticuloDTO>(resultado);
                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultado);
            }
        }

        private void FiltrarArticulos()
        {
            if (_todosLosArticulos == null) return;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                ListaArticulos = new ObservableCollection<ArticuloDTO>(_todosLosArticulos);
            }
            else
            {
                var texto = TextoBusqueda.ToLower();
                var resultados = _todosLosArticulos
                    .Where(a => (a.Descripcion != null && a.Descripcion.ToLower().Contains(texto)) ||
                               (a.IdCategoria.ToString().Contains(texto)) ||
                               (a.idArticulo.ToString().Contains(texto)) ||
                               (a.Color != null && a.Color.ToLower().Contains(texto)))
                    .ToList();

                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultados);
            }
        }



        private void BotonAgregar(object sender, RoutedEventArgs e)
        {

            ArticuloSeleccionado = new ArticuloDTO();
            AgregarPopup.IsOpen = true;
        }

        private async void BotonQuitar(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado != null)
            {
                MessageBoxResult resultado = MessageBox.Show(
                    $"¿Eliminar {ArticuloSeleccionado.Descripcion}?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    bool eliminado = await _apiService.DeleteAsync($"api/CArticulos/{ArticuloSeleccionado.idArticulo}");

                    if (eliminado)
                    {
                        ListaArticulos.Remove(ArticuloSeleccionado);
                        MessageBox.Show("Artículo eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Error al eliminar el artículo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un artículo para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async void BotonGuardarPopup(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null)
            {
                MessageBox.Show("No hay artículo seleccionado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(ArticuloSeleccionado.Foto))
            {
                MessageBox.Show("Debe seleccionar una imagen.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ArticuloSeleccionado.PrecioVenta <= 0)
            {
                MessageBox.Show("El Precio de Venta debe ser mayor a 0.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var resultado = await _apiService.PostAsync("api/CArticulos/", ArticuloSeleccionado);

            if (resultado)
            {
                ListaArticulos.Add(ArticuloSeleccionado);
                MessageBox.Show("Artículo agregado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                AgregarPopup.IsOpen = false;
            }
            else
            {
                MessageBox.Show("Error al guardar el artículo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BotonCancelarPopup(object sender, RoutedEventArgs e)
        {

            AgregarPopup.IsOpen = false;
        }

        private void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.gif";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    string resourcesDir = System.IO.Path.Combine(appDir, "Resources");

                    if (!Directory.Exists(resourcesDir))
                        Directory.CreateDirectory(resourcesDir);

                    string extension = System.IO.Path.GetExtension(openFileDialog.FileName);
                    string nombreImagen = $"art_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    string rutaDestino = System.IO.Path.Combine(resourcesDir, nombreImagen);

                    File.Copy(openFileDialog.FileName, rutaDestino, true);

                    TxtImagenRuta.Text = nombreImagen;
                    ArticuloSeleccionado.Foto = nombreImagen;

                    OnPropertyChanged(nameof(ArticuloSeleccionado));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la imagen: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
