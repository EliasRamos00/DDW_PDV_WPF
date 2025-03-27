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
using Newtonsoft.Json;
using System.Net.Http;
using System.Xml.Linq;


namespace DDW_PDV_WPF
{



    public partial class frmInventarios : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ApiService _apiService;
        private ObservableCollection<ArticuloDTO> _listaArticulos;
        private ArticuloDTO _articuloSeleccionado;
        private bool _hasChanges;
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
                btnCancelarCambios.Visibility = Visibility.Hidden;
                 btnGuardarCambios.Visibility = Visibility.Hidden;
                OnPropertyChanged(nameof(ArticuloSeleccionado)); // Notificar cambios
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
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

            // Inicializar ApiService
            _apiService = new ApiService();
            CargarDatos();
            DataContext = this;

            btnCancelarCambios.Visibility = Visibility.Hidden;
            btnGuardarCambios.Visibility = Visibility.Hidden;

        }



        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasChanges = true;
           
        }
        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado != null)
            {
                // Restaurar valores originales
                var articuloOriginal = _todosLosArticulos.FirstOrDefault(a => a.idArticulo == ArticuloSeleccionado.idArticulo);
                if (articuloOriginal != null)
                {
                    ArticuloSeleccionado = new ArticuloDTO
                    {
                        // Propiedades básicas del artículo
                        idArticulo = articuloOriginal.idArticulo,
                        Foto = articuloOriginal.Foto,
                        Color = articuloOriginal.Color,
                        Descripcion = articuloOriginal.Descripcion,
                        Tamanio = articuloOriginal.Tamanio,
                        CodigoBarras = articuloOriginal.CodigoBarras,
                        IdCategoria = articuloOriginal.IdCategoria,

                        // Propiedades de inventario
                        idInventario = articuloOriginal.idInventario,
                        Stock = articuloOriginal.Stock,
                        Min = articuloOriginal.Min,
                        Max = articuloOriginal.Max,
                        PrecioVenta = articuloOriginal.PrecioVenta,
                        PrecioCompra = articuloOriginal.PrecioCompra,

                    };
                }
            }

            HasChanges = false;
            btnCancelarCambios.Visibility = Visibility.Hidden;
            btnGuardarCambios.Visibility = Visibility.Hidden;
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null)
            {
                MessageBox.Show("Seleccione un artículo para guardar cambios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(ArticuloSeleccionado.Descripcion))
                {
                    MessageBox.Show("La descripción es obligatoria", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

               
                bool resultado = await _apiService.PutAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}", ArticuloSeleccionado);

                if (resultado)
                {
          
                     CargarDatos();
                    MessageBox.Show("Artículo actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    btnCancelarCambios.Visibility = Visibility.Hidden;
                    btnGuardarCambios.Visibility = Visibility.Hidden;
                    HasChanges = false;
                }
                else
                {
                    MessageBox.Show("Error al actualizar el artículo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cambios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrEmpty(value as string);
            if (parameter?.ToString() == "inverse") return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }
        private async void CargarDatos()
        {
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("/api/CArticulos");

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
        private async void BotonGuardarPopup(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar datos obligatorios antes de enviar
                if (string.IsNullOrEmpty(ArticuloSeleccionado.Descripcion))
                {
                    MessageBox.Show("La descripción es obligatoria", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AgregarPopup.IsOpen = true;
                    return;
                }

                // Usar el endpoint correcto para crear artículos
                var response = await _apiService.PostAsync("/api/CArticulos", ArticuloSeleccionado);

                if (response)
                {
                    ListaArticulos.Add(ArticuloSeleccionado);
                    MessageBox.Show("Artículo agregado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    AgregarPopup.IsOpen = false;
                }
                else
                {
                    MessageBox.Show("El servidor rechazó la solicitud. Revisa los datos enviados.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AgregarPopup.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hubo un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AgregarPopup.IsOpen = true;
            }
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
                    bool eliminado = await _apiService.DeleteAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}");

                    if (eliminado)
                    {
                        ListaArticulos.Remove(ArticuloSeleccionado);
                        MessageBox.Show("Artículo eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                       
                    }
                    else
                    {
                        MessageBox.Show("Error al eliminar el artículo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        AgregarPopup.IsOpen = true;
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un artículo para eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
      





        private void BotonCancelarPopup(object sender, RoutedEventArgs e)
        {
            AgregarPopup.IsOpen = false;
        }

        private void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Seleccionar imagen del artículo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 1. Ruta de destino (carpeta Resources del proyecto)
                    string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                    string resourcesPath = System.IO.Path.Combine(projectRoot, "Resources");

                    // 2. Crear directorio si no existe
                    if (!Directory.Exists(resourcesPath))
                        Directory.CreateDirectory(resourcesPath);

                    // 3. Generar nombre único para evitar colisiones
                    string fileName = $"art_{DateTime.Now:yyyyMMddHHmmss}{System.IO.Path.GetExtension(openFileDialog.FileName)}";
                    string finalPath = System.IO.Path.Combine(resourcesPath, fileName);

                    // 4. Copiar el archivo
                    File.Copy(openFileDialog.FileName, finalPath, overwrite: true);

                    // 5. Guardar ruta relativa en el modelo
                    ArticuloSeleccionado.Foto = $"Resources\\{fileName}";

                    // 6. Actualizar UI
                    OnPropertyChanged(nameof(ArticuloSeleccionado));

                    // 7. (NUEVO) Añadir el archivo al .csproj para que aparezca en el Explorador de Soluciones
                    AddFileToProject(System.IO.Path.Combine("Resources", fileName));

                    MessageBox.Show("Imagen guardada correctamente", "Éxito",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    AgregarPopup.IsOpen = true;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la imagen: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
       
        private void AddFileToProject(string relativePath)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string projectFile = Directory.GetFiles(projectRoot, "*.csproj").FirstOrDefault();

            if (projectFile != null)
            {
                XDocument doc = XDocument.Load(projectFile);
                XNamespace ns = doc.Root.GetDefaultNamespace();

                // Verificar si el archivo ya está incluido
                bool alreadyIncluded = doc.Descendants(ns + "Content")
                                         .Any(x => x.Attribute("Include")?.Value == relativePath);

                if (!alreadyIncluded)
                {
                    doc.Root.Add(new XElement(ns + "ItemGroup",
                        new XElement(ns + "Content",
                            new XAttribute("Include", relativePath),
                            new XElement(ns + "CopyToOutputDirectory", "PreserveNewest")
                        )
                    ));
                    doc.Save(projectFile);
                }
            }
        }

        private void txtDescripcion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HasChanges = true;
            btnCancelarCambios.Visibility = Visibility.Visible;
            btnGuardarCambios.Visibility = Visibility.Visible;
        }
    }
}
