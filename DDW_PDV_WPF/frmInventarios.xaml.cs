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
using System.Windows.Media.Imaging;

namespace DDW_PDV_WPF
{



    public partial class frmInventarios : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispatcherTimer _timer;
        private readonly ApiService _apiService;
        private ObservableCollection<ArticuloDTO> _listaArticulos;
        private ArticuloDTO _articuloSeleccionado;

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

        public frmInventarios()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Actualizar cada segundo
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Inicializar ApiService
            _apiService = new ApiService();
            CargarDatos();
            DataContext = this;

            // Actualizar la fecha y hora al iniciar
            UpdateDateTime();
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            // Actualizar la fecha y hora en cada tick del timer
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            // Obtener la fecha y hora actual
            DateTime now = DateTime.Now;

            // Formatear la fecha
            txtFecha.Text = now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"));

            // Formatear la hora con minutos y segundos
            txtHora.Text = now.ToString("HH:mm:ss", new CultureInfo("es-MX"));
        }

        private async void CargarDatos()
        {
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("api/CArticulos/");

            if (resultado != null)
            {
                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultado); // Asigna los datos a la propiedad
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
                string imagenSeleccionada = openFileDialog.FileName;


                string destinoCarpeta = System.IO.Path.Combine(Directory.GetCurrentDirectory(),"Imagenes");


                if (!Directory.Exists(destinoCarpeta))
                {
                    Directory.CreateDirectory(destinoCarpeta);
                }


                string nombreImagen =    System.IO.Path.GetFileName(imagenSeleccionada);
                string rutaDestino = System.IO.Path.Combine(destinoCarpeta, nombreImagen);

                try
                {
                    File.Copy(imagenSeleccionada, rutaDestino, true); 

                    TxtImagenRuta.Text = System.IO.Path.Combine("Imagenes", nombreImagen);
                    ArticuloSeleccionado.Foto = rutaDestino;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




    }
}
