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
using System.Drawing.Imaging;
using System.Drawing;


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
        private List<MCategorias> cat;
        private GoogleDriveHelper ds;
       

        private ObservableCollection<MCategorias> _categorias;
        public ObservableCollection<MCategorias> Categorias
        {
            get => _categorias;
            set
            {
                _categorias = value;
                OnPropertyChanged(nameof(Categorias));
            }
        }
        public ObservableCollection<ArticuloDTO> ListaArticulos
        {
            get { return _listaArticulos; }
            set
            {
                _listaArticulos = value;
                OnPropertyChanged(nameof(ListaArticulos));
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
                        idCategoria = _articuloSeleccionado.idCategoria,
                        idInventario = _articuloSeleccionado.idInventario,
                        Stock = _articuloSeleccionado.Stock,
                        Min = _articuloSeleccionado.Min,
                        Max = _articuloSeleccionado.Max,
                        PrecioVenta = _articuloSeleccionado.PrecioVenta,
                        PrecioCompra = _articuloSeleccionado.PrecioCompra,
                        ImagenProducto = _articuloSeleccionado.ImagenProducto

                    };
                    // Cargar la categoría seleccionada
                    // Buscar el objeto original

                    MCategorias itemEncontrado = cmbCategoria.ItemsSource.Cast<MCategorias>()
                     .FirstOrDefault(c => c.idCategoria == _articuloSeleccionado.idCategoria);

                    cmbCategoria.SelectedItem = itemEncontrado;

                    // Se muestra la foto que tenga.
                    // Suponemos que cada artículo tiene un "ImageId" que corresponde al ID de Google Drive de la imagen

                    CargarImagenDelArticulo(_articuloSeleccionado);



                }


                btnCancelarCambios.Visibility = value != null ? Visibility.Visible : Visibility.Hidden;
                btnGuardarCambios.Visibility = value != null ? Visibility.Visible : Visibility.Hidden;

                OnPropertyChanged(nameof(ArticuloSeleccionado));
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        private async void CargarImagenDelArticulo(ArticuloDTO articuloSeleccionado)
        {
            try
            {
                string fileId = articuloSeleccionado.Foto;
                if (fileId == "")
                {
                    fileId = "1RQQ0GNUWuLijKIKnZmYwbBh80eQpD9vp";
                }


                string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";

                var imageSource = await ds.GetImageFromCacheOrDownload(downloadUrl, fileId);
                articuloSeleccionado.ImagenProducto = imageSource;

                OnPropertyChanged(nameof(ArticuloSeleccionado)); // Notificar cambio visual
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando imagen: " + ex.Message);
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
                FiltrarArticulos();
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
        public frmInventarios(GoogleDriveHelper ds)
        {
            InitializeComponent();
           
            _apiService = new ApiService();
            CargarDatos();
            DataContext = this;

            CargarCategorias();

            btnCancelarCambios.Visibility = Visibility.Hidden;
            btnGuardarCambios.Visibility = Visibility.Hidden;

            this.ds = ds;


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
                CargarDatos();
                OnPropertyChanged(nameof(ArticuloSeleccionado));
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
            await CargarCategorias();
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("/api/CArticulos/productos/inventario");

            if (resultado != null)
            {
                _todosLosArticulos = new ObservableCollection<ArticuloDTO>(resultado);
                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultado);
                ArticuloSeleccionado = _todosLosArticulos.FirstOrDefault();
            }
        }

        public static ImageSource ConvertBitmapToImageSource(Bitmap bitmapTask)
        {
            Bitmap bitmap = bitmapTask;

            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Para que se pueda usar en hilos distintos al UI

                return bitmapImage;
            }
        }


        private async Task CargarCategorias()
        {
            try
            {
                cat = await _apiService.GetAsync<List<MCategorias>>("/api/CCategorias");
                if (cat != null)
                {

                    cmbCategoria.ItemsSource = cat; // Limpiar el ItemsSource antes de asignar
                    cmbCategoria.DisplayMemberPath = "Nombre";     // Lo que se muestra
                    cmbCategoria.SelectedValuePath = "Id";
                    //Categorias = new ObservableCollection<MCategorias>(resultado);
                    //_cbCategorias.ItemsSource = Categorias;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
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
                               (a.idCategoria.ToString().Contains(texto)) ||
                               (a.idArticulo.ToString().Contains(texto)) ||
                               (a.Color != null && a.Color.ToLower().Contains(texto)))
                    .ToList();

                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultados);
            }
        }



        private void BotonAgregar(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Se va esta por crear un nuevo articulo, introduzca la informacion en el lado derecho", "Creacion de nuevo articulo", MessageBoxButton.OK, MessageBoxImage.Information);
          
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


        public string LimpiarPrecio(string precioFormateado)
        {
            // Quita todo lo que no sea número o separador decimal
            var limpio = new string(precioFormateado
                .Where(c => char.IsDigit(c) || c == '.' || c == ',')
                .ToArray());

            // Normaliza la coma a punto si la coma es usada como decimal
            if (limpio.Count(c => c == ',') == 1 && !limpio.Contains('.'))
                limpio = limpio.Replace(',', '.');
            else
                limpio = limpio.Replace(",", ""); // Quita comas de miles

            return limpio;
        }

     
        private async void GuardarCambios(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null) return;

            // Validaciones de campos vacíos
            if (string.IsNullOrWhiteSpace(ArticuloSeleccionado.Descripcion))
            {
                MessageBox.Show("La descripción no puede estar vacía.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ArticuloSeleccionado.Color))
            {
                MessageBox.Show("El color no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ArticuloSeleccionado.Tamanio))
            {
                MessageBox.Show("El tamaño no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (ArticuloSeleccionado.Foto == "")
            {
                ArticuloSeleccionado.Foto = "1RQQ0GNUWuLijKIKnZmYwbBh80eQpD9vp";
            }

            if (cmbCategoria.SelectedItem == null)
            {
                MessageBox.Show("Debes seleccionar una categoría.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener los valores directamente de los TextBox
            if (!int.TryParse(txtStock.Text, out int stock) ||
                !int.TryParse(txtMin.Text, out int min) ||
                !int.TryParse(txtMax.Text, out int max))
            {
                MessageBox.Show("Stock, mínimo y máximo deben ser números enteros válidos.",
                               "Validación",
                               MessageBoxButton.OK,
                               MessageBoxImage.Warning);
                return;
            }

            // Solo asignar los valores si pasan la validación
            ArticuloSeleccionado.Stock = stock;
            ArticuloSeleccionado.Min = min;
            ArticuloSeleccionado.Max = max;

            // Validar precios, limpiando primero
            string precioVenta = LimpiarPrecio(txtPrecioVenta.Text);
            string precioCompra = LimpiarPrecio(txtPrecioCompra.Text);

            if (!decimal.TryParse(precioVenta, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precioV) ||
                !decimal.TryParse(precioCompra, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precioC))
            {
                MessageBox.Show("Los precios deben ser valores numéricos válidos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Asignar valores numéricos ya validados al artículo
            ArticuloSeleccionado.PrecioVenta = precioV;
            ArticuloSeleccionado.PrecioCompra = precioC;

            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("El código de barras no puede estar vacío.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var usuarioActual = Properties.Settings.Default.idUsuario;
               

                // Asignar categoría
                MCategorias aux = (MCategorias)cmbCategoria.SelectedItem;
                ArticuloSeleccionado.idCategoria = aux.idCategoria;

                bool exito;
                string accion = _isNewItem ? "CREADO" : "ACT.";

                if (_isNewItem)
                {
                    exito = await _apiService.PostAsync("/api/CArticulos", ArticuloSeleccionado);
                }
                else
                {
                    exito = await _apiService.PutAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}", ArticuloSeleccionado);
                }

                if (exito)
                {
                    var historial = new HistorialDTO
                    {
                        fechaHora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                        idUsuario = Properties.Settings.Default.idUsuario,
                        accion = accion,
                        clase = "Inventarios",
                        antes = SerializarAXml(_articuloOriginal),
                        despues = SerializarAXml(ArticuloSeleccionado)
                    };

                    bool historialExito = await _apiService.PostAsync("/api/CHistoriales", historial);

                    string mensaje = historialExito
                        ? $"Operación completada  con registro en el historial."
                        : $"Operación completada , pero falló el registro en el historial.";

                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    _isNewItem = false;
                    CargarDatos();
                    HasChanges = false;
                    btnCancelarCambios.Visibility = Visibility.Hidden;
                    btnGuardarCambios.Visibility = Visibility.Hidden;
                    txtBusqueda.Text ="";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite solo números, punto decimal y un solo signo negativo al inicio
            var textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            e.Handled = !double.TryParse(newText, NumberStyles.AllowDecimalPoint |
                                                NumberStyles.AllowLeadingSign,
                                                CultureInfo.CurrentCulture, out _);

           
        }
        private async void EliminarArticulo(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Está seguro de eliminar el artículo {ArticuloSeleccionado.idArticulo}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                // Obtener usuario actual completo
                var usuarioActual = Properties.Settings.Default.idUsuario;
                

                // Registrar en historial antes de eliminar
                var historial = new HistorialDTO
                {
                    fechaHora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    idUsuario = Properties.Settings.Default.idUsuario,
                    accion = "ELIM.",
                    clase = "ArticulosDTO",
                    antes = SerializarAXml(ArticuloSeleccionado),
                    despues = null
                };

                bool historialExito = await _apiService.PostAsync("/api/CHistoriales", historial);

                // Eliminar el artículo
                bool exito = await _apiService.DeleteAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}");

                if (exito)
                {
                    string mensaje = historialExito
                        ? $"Se eliminó el artículo con registro en el historial."
                        : $"Se eliminó el artículo, pero falló el registro en el historial.";

                    MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarDatos();
                    txtBusqueda.Text = "";
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private async void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null)
            {
                MessageBox.Show("Seleccione o cree un artículo primero.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Seleccionar imagen del artículo"
            };
            var copiaImageSource = new BitmapImage();
            // Si se selecciona una imagen, se sube a Google Drive
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    btnGuardarCambios.IsEnabled = false;
                    // Obtener el servicio autenticado de Google Drive
                    Controlador.GoogleDriveHelper.Initialize("neuralcat.json"); // AQUI TENGO QUE INICIAR SESION PARA SUBIR.

                    // ID de la carpeta compartida en tu Google Drive
                    string folderId = "1mljTxnPYGefWWFBbWe2V_lKxX7oeugdA"; // ID fijo de la carpeta

                    // Sube el archivo y obtén el fileId
                    string fileId = await Controlador.GoogleDriveHelper.UploadFileAsync(filePath, folderId);

                    // Hacerlo publico
                    await GoogleDriveHelper.MakeFilePublicAsync(fileId);


                    // Obtener el enlace público (opcional)
                    string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";

                    // Asignar el fileId al artículo seleccionado para guardarlo en la base de datos
                    ArticuloSeleccionado.Foto = fileId;

                    // Usar el helper para obtener la imagen desde la caché o descargarla
                    var imageSource = await ds.GetImageFromCacheOrDownload(downloadUrl, fileId);

                    // Asignar la imagen al artículo (o directamente en el UI si tienes un control de imagen)
                    ArticuloSeleccionado.ImagenProducto = imageSource;
                    copiaImageSource = imageSource;


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al subir archivo: " + ex.Message);
                    return;
                }
                finally
                {
                    btnGuardarCambios.IsEnabled = true;
                    // Actualizar UI
                    OnPropertyChanged(nameof(ArticuloSeleccionado));
                    HasChanges = true;
                    btnCancelarCambios.Visibility = Visibility.Visible;
                    btnGuardarCambios.Visibility = Visibility.Visible;
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

        private void BtnExportarQR_Click(object sender, RoutedEventArgs e)
        {
            if (imgCodigoQR.Source == null)
            {
                MessageBox.Show("No hay código QR generado para exportar.", "Advertencia",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Generar nombre de archivo seguro
            string nombreBase = "QR_generado";
            if (ArticuloSeleccionado != null && ArticuloSeleccionado.idArticulo > 0)
            {
                nombreBase = $"QR_{ArticuloSeleccionado.idArticulo}";
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Imagen PNG|*.png|Imagen JPEG|*.jpg|Todos los archivos|*.*",
                Title = "Exportar código QR",
                FileName = $"{nombreBase}_{DateTime.Now:yyyyMMddHHmmss}",
                DefaultExt = ".png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    BitmapEncoder encoder;
                    if (saveFileDialog.FileName.EndsWith(".jpg"))
                    {
                        encoder = new JpegBitmapEncoder();
                    }
                    else
                    {
                        encoder = new PngBitmapEncoder();
                    }

                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgCodigoQR.Source));

                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }

                    MessageBox.Show("Código QR exportado correctamente.", "Éxito",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar el código QR: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void txtDescripcion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            HasChanges = true;
            btnCancelarCambios.Visibility = Visibility.Visible;
            btnGuardarCambios.Visibility = Visibility.Visible;
        }

        private void cmbCategoriaCambio(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategoria.SelectedValue != null)
            {
                MCategorias aux = (MCategorias)cmbCategoria.SelectedValue;
                _articuloSeleccionado.idCategoria = aux.idCategoria;
            }

        }
    }
}
