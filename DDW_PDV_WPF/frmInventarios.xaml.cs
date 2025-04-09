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
using System.Diagnostics;
using QRCoder;
using System.Xml.Serialization;


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
        private ArticuloDTO _articuloOriginal;
        private bool _isNewItem = false;



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
            get => _articuloSeleccionado;
            set
            {
                _articuloSeleccionado = value;
               

                if (_articuloSeleccionado != null && !_isNewItem && value != null)
                {
                    _articuloOriginal = new ArticuloDTO
                    {
                        idArticulo = _articuloSeleccionado.idArticulo,
                        Foto = _articuloSeleccionado.Foto,
                        Color = _articuloSeleccionado.Color,
                        Descripcion = _articuloSeleccionado.Descripcion,
                        Tamanio = _articuloSeleccionado.Tamanio,
                        CodigoBarras = _articuloSeleccionado.CodigoBarras,
                        IdCategoria = _articuloSeleccionado.IdCategoria,
                        idInventario = _articuloSeleccionado.idInventario,
                        Stock = _articuloSeleccionado.Stock,
                        Min = _articuloSeleccionado.Min,
                        Max = _articuloSeleccionado.Max,
                        PrecioVenta=_articuloSeleccionado.PrecioVenta,
                        PrecioCompra=_articuloSeleccionado.PrecioCompra

                    };
                }

                
                btnCancelarCambios.Visibility = value != null ? Visibility.Visible : Visibility.Hidden;
                btnGuardarCambios.Visibility = value != null ? Visibility.Visible : Visibility.Hidden;

                OnPropertyChanged(nameof(ArticuloSeleccionado));
                OnPropertyChanged(nameof(HasChanges));
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

            _apiService = new ApiService();
            CargarDatos();
            DataContext = this;

            btnCancelarCambios.Visibility = Visibility.Hidden;
            btnGuardarCambios.Visibility = Visibility.Hidden;

        }

        private void txtCodigo_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textoQR = txtCodigo.Text; 
            BitmapImage qrImage = GenerarCodigoQR(textoQR);

            if (qrImage != null)
            {
                imgCodigoQR.Source = qrImage;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HasChanges = true;

        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewItem)
            {
                ArticuloSeleccionado = null;
                _isNewItem = false;
            }
            else if (ArticuloSeleccionado != null)
            {
            
                int idSeleccionado = ArticuloSeleccionado.idArticulo;
                CargarDatos();
                ArticuloSeleccionado = _todosLosArticulos.FirstOrDefault(a => a.idArticulo == idSeleccionado);
                OnPropertyChanged(nameof(ArticuloSeleccionado));
            }

            HasChanges = false;
            btnCancelarCambios.Visibility = Visibility.Hidden;
            btnGuardarCambios.Visibility = Visibility.Hidden;
        }

        private BitmapImage GenerarCodigoQR(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return null;

            // Generar el QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            // Convertir a BitmapImage para WPF
            using (MemoryStream stream = new MemoryStream(qrCodeBytes))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                return image;
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
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("/api/CArticulos/productos/inventario");

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
            // Crear nuevo artículo
            ArticuloSeleccionado = new ArticuloDTO();
            _isNewItem = true;

            // Mostrar botones de guardar/cancelar
            btnCancelarCambios.Visibility = Visibility.Visible;
            btnGuardarCambios.Visibility = Visibility.Visible;

            // Establecer valores por defecto
            ArticuloSeleccionado.Stock = 0;
            ArticuloSeleccionado.Min = 0;
            ArticuloSeleccionado.Max = 0;
            ArticuloSeleccionado.PrecioVenta = 0;
            ArticuloSeleccionado.PrecioCompra = 0;

            // Notificar cambios
            OnPropertyChanged(nameof(ArticuloSeleccionado));

        }

        private string SerializarAXml(ArticuloDTO articulo)
        {
            if (articulo == null) return null;

            try
            {
                var serializer = new XmlSerializer(typeof(ArticuloDTO));
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, articulo);
                    return writer.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al serializar a XML: {ex.Message}");
                return null;
            }
        }

       
        private async void GuardarCambios(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null) return;

            try
            {
                bool success;
                string accion = _isNewItem ? "CREATE" : "UPDATE";

                // Guardar el artículo
                if (_isNewItem)
                {
                    success = await _apiService.PostAsync("/api/CArticulos", ArticuloSeleccionado);
                }
                else
                {
                    success = await _apiService.PutAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}", ArticuloSeleccionado);
                }

                if (success)
                {
                    var historial = new HistorialDTO
                    {
                        fechaHora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        idUsuario = "1",
                        accion = accion,
                        clase = "ArticulosDTO",
                        antes =  SerializarAXml(_articuloOriginal),
                        despues = SerializarAXml(ArticuloSeleccionado)
                    };

                    bool historialSuccess = await _apiService.PostAsync("/api/CHistoriales", historial);

                    string mensaje = historialSuccess
                        ? "Operación completada con registro de historial"
                        : "Operación completada pero falló registro de historial";

                    MessageBox.Show(mensaje);

                    _isNewItem = false;
                    CargarDatos();
                    HasChanges = false;
                    btnCancelarCambios.Visibility = Visibility.Hidden;
                    btnGuardarCambios.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

       

        private async void EliminarArticulo(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Está seguro que desea eliminar el artículo {ArticuloSeleccionado.idArticulo}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                // Registrar en historial antes de eliminar
                var historial = new HistorialDTO
                {
                    fechaHora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    idUsuario = "1",
                    accion = "DELETE",
                    clase = "ArticulosDTO",
                    antes = SerializarAXml(ArticuloSeleccionado),
                    despues = null
                };

                bool historialSuccess = await _apiService.PostAsync("/api/CHistoriales", historial);

                // Eliminar el artículo
                bool success = await _apiService.DeleteAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}");

                if (success)
                {
                    string mensaje = historialSuccess
                        ? "Artículo eliminado con registro de historial"
                        : "Artículo eliminado pero falló registro de historial";

                    MessageBox.Show(mensaje);
                    CargarDatos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}");
            }
        }


        private void PrecioTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Quitar el formato monetario al recibir el foco
                if (decimal.TryParse(textBox.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal valor))
                {
                    textBox.Text = valor.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void PrecioTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Aplicar formato monetario al perder el foco
                if (decimal.TryParse(textBox.Text, out decimal valor))
                {
                    textBox.Text = valor.ToString("C");
                }
                else
                {
                    textBox.Text = 0.ToString("C");
                }
            }
        }
        private void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null)
            {
                MessageBox.Show("Seleccione o cree un artículo primero.", "Advertencia",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    ArticuloSeleccionado.Foto = $"Resources/{fileName}";

                    // 6. Actualizar UI
                    OnPropertyChanged(nameof(ArticuloSeleccionado));
                    HasChanges = true;
                    btnCancelarCambios.Visibility = Visibility.Visible;
                    btnGuardarCambios.Visibility = Visibility.Visible;

                    // 7. Añadir el archivo al .csproj
                    AddFileToProject(System.IO.Path.Combine("Resources", fileName));

                    MessageBox.Show("Imagen guardada correctamente", "Éxito",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar la imagen: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnEliminarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null || string.IsNullOrEmpty(ArticuloSeleccionado.Foto))
                return;

            MessageBoxResult result = MessageBox.Show("¿Está seguro que desea eliminar la imagen de este artículo?",
                                                    "Confirmar eliminación",
                                                    MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Eliminar el archivo físico
                    string fullPath = System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName,
                                                 ArticuloSeleccionado.Foto);

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }

                    // Limpiar la referencia en el objeto
                    ArticuloSeleccionado.Foto = null;

                    // Actualizar UI
                    OnPropertyChanged(nameof(ArticuloSeleccionado));
                    HasChanges = true;
                    btnCancelarCambios.Visibility = Visibility.Visible;
                    btnGuardarCambios.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la imagen: {ex.Message}", "Error",
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
                try
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
                catch (Exception ex)
                {
                    // No es crítico si falla, solo es para conveniencia en el desarrollo
                    Debug.WriteLine($"Error al añadir archivo al proyecto: {ex.Message}");
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
