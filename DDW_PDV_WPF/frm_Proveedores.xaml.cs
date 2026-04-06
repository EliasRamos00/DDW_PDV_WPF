using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DDW_PDV_WPF
{
    public class ProveedorUI : INotifyPropertyChanged
    {
        public int IdProveedor { get; set; }

        private string _nombre;
        public string Nombre { get => _nombre; set { _nombre = value; OnPropertyChanged(nameof(Nombre)); } }

        private string _correo;
        public string Correo { get => _correo; set { _correo = value; OnPropertyChanged(nameof(Correo)); } }

        private string _telefono;
        public string Telefono { get => _telefono; set { _telefono = value; OnPropertyChanged(nameof(Telefono)); } }

        private string _direccion;
        public string Direccion { get => _direccion; set { _direccion = value; OnPropertyChanged(nameof(Direccion)); } }

        private string _estado;
        public string Estado { get => _estado; set { _estado = value; OnPropertyChanged(nameof(Estado)); } }

        private string _foto;
        public string Foto { get => _foto; set { _foto = value; OnPropertyChanged(nameof(Foto)); } }

        private ImageSource _imagenProveedor;
        public ImageSource ImagenProveedor { get => _imagenProveedor; set { _imagenProveedor = value; OnPropertyChanged(nameof(ImagenProveedor)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ComandoLocal : ICommand
    {
        private readonly Action _execute;
        public ComandoLocal(Action execute) { _execute = execute; }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged { add { } remove { } }
    }

    public partial class frm_Proveedores : Page, INotifyPropertyChanged
    {
        // --- CONSTANTES ---
        private const string ID_IMAGEN_POR_DEFECTO = "1RQQ0GNUWuLijKIKnZmYwbBh80eQpD9vp";
        private const string ID_CARPETA_DRIVE = "1mljTxnPYGefWWFBbWe2V_lKxX7oeugdA";

        private readonly ApiService _apiService;
        private readonly GoogleDriveHelper ds;

        private ObservableCollection<ProveedorUI> _listaProveedores;
        private ProveedorUI _proveedorSeleccionado;
        private bool _hasChanges;
        private bool _isNewItem = false;
        private string _textoBusqueda;

        private ProveedorUI _proveedorOriginalEnLista;

        // --- MEMORIA PARA IMÁGENES ---
        private ImageSource _imagenOriginalDeRespaldo;
        private string _rutaImagenLocal;

        private bool _ignorandoSeleccion = false;

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));
                AplicarFiltroBusqueda();
            }
        }

        public ICommand LimpiarBusquedaCommand => new ComandoLocal(() => TextoBusqueda = string.Empty);

        public ObservableCollection<ProveedorUI> ListaProveedores
        {
            get => _listaProveedores;
            set { _listaProveedores = value; OnPropertyChanged(nameof(ListaProveedores)); }
        }

        public ProveedorUI ProveedorSeleccionado
        {
            get => _proveedorSeleccionado;
            set
            {
                _proveedorSeleccionado = value;
                OnPropertyChanged(nameof(ProveedorSeleccionado));
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
                if (btnGuardarCambios != null) btnGuardarCambios.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (btnCancelarCambios != null) btnCancelarCambios.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (btnEditar != null) btnEditar.Visibility = value ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public frm_Proveedores(GoogleDriveHelper ds)
        {
            InitializeComponent();
            this.ds = ds;

            Controlador.GoogleDriveHelper.Initialize("neuralcat.json");

            _apiService = new ApiService();
            ListaProveedores = new ObservableCollection<ProveedorUI>();

            this.Loaded += (s, e) => {
                this.DataContext = this;
                HasChanges = false;
                CargarProveedoresDesdeAPI();
            };

            BotonAgregar1.Click += BotonAgregar_Click;
            btnEditar.Click += btnEditar_Click;
            btnGuardarCambios.Click += btnGuardarCambios_Click;
            btnCancelarCambios.Click += btnCancelarCambios_Click;
            btnToggleEstado.Click += btnToggleEstado_Click;

            // --- SISTEMA DE ALERTA DE DATOS SIN GUARDAR ---
            lstProveedores.SelectionChanged += (s, e) =>
            {
                if (_ignorandoSeleccion) return;

                if (HasChanges)
                {
                    var respuesta = MessageBox.Show(
                        "Detectamos que tienes cambios sin guardar.\n\n" +
                        "[Aceptar] = Descartar los cambios y ver a otro proveedor.\n" +
                        "[Cancelar] = Quedarse aquí y continuar editando.",
                        "Cambios sin guardar",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                    if (respuesta == MessageBoxResult.Cancel)
                    {
                        // EL USUARIO SE QUEDA. Restauramos la selección de la lista.
                        _ignorandoSeleccion = true;
                        lstProveedores.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                        _ignorandoSeleccion = false;

                        // ==========================================
                        // SOLUCIÓN AL BUG DE LA IMAGEN INVISIBLE:
                        // ==========================================
                        if (ProveedorSeleccionado != null)
                        {
                            // Si el usuario había subido una foto nueva, la volvemos a leer de la PC a la fuerza
                            if (!string.IsNullOrEmpty(_rutaImagenLocal))
                            {
                                try
                                {
                                    BitmapImage bitmapFuerte = new BitmapImage();
                                    bitmapFuerte.BeginInit();
                                    bitmapFuerte.UriSource = new Uri(_rutaImagenLocal);
                                    bitmapFuerte.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmapFuerte.EndInit();
                                    bitmapFuerte.Freeze();

                                    ProveedorSeleccionado.ImagenProveedor = bitmapFuerte;
                                }
                                catch { }
                            }
                            else
                            {
                                // Si no había foto nueva, forzamos un repintado de la anterior
                                var imgTemp = ProveedorSeleccionado.ImagenProveedor;
                                ProveedorSeleccionado.ImagenProveedor = null;
                                ProveedorSeleccionado.ImagenProveedor = imgTemp;
                            }
                        }

                        return; // Cortamos el proceso para no perder los cambios
                    }
                    else
                    {
                        // EL USUARIO DESCARTA LOS CAMBIOS (Presionó Aceptar).
                        HasChanges = false;
                        _isNewItem = false;
                        _rutaImagenLocal = null;
                    }
                }

                // Cargar al nuevo proveedor
                if (lstProveedores.SelectedItem != null)
                {
                    _proveedorOriginalEnLista = lstProveedores.SelectedItem as ProveedorUI;

                    ProveedorSeleccionado = new ProveedorUI
                    {
                        IdProveedor = _proveedorOriginalEnLista.IdProveedor,
                        Nombre = _proveedorOriginalEnLista.Nombre,
                        Correo = _proveedorOriginalEnLista.Correo,
                        Telefono = _proveedorOriginalEnLista.Telefono,
                        Direccion = _proveedorOriginalEnLista.Direccion,
                        Estado = _proveedorOriginalEnLista.Estado,
                        Foto = _proveedorOriginalEnLista.Foto
                    };

                    _rutaImagenLocal = null;
                    CargarImagenDelProveedor(ProveedorSeleccionado);
                    HasChanges = false;
                }
            };
        }

        // --- MÉTODO DE BÚSQUEDA ---
        private void AplicarFiltroBusqueda()
        {
            ICollectionView vista = CollectionViewSource.GetDefaultView(ListaProveedores);
            if (vista != null)
            {
                if (string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    vista.Filter = null;
                }
                else
                {
                    vista.Filter = obj =>
                    {
                        var prov = obj as ProveedorUI;
                        if (prov == null) return false;

                        return (prov.Nombre != null && prov.Nombre.IndexOf(TextoBusqueda, StringComparison.OrdinalIgnoreCase) >= 0) ||
                               (prov.Estado != null && prov.Estado.IndexOf(TextoBusqueda, StringComparison.OrdinalIgnoreCase) >= 0);
                    };
                }
            }
        }

        private async void CargarImagenDelProveedor(ProveedorUI proveedor)
        {
            try
            {
                string fileId = proveedor.Foto;
                if (string.IsNullOrEmpty(fileId))
                {
                    fileId = ID_IMAGEN_POR_DEFECTO;
                }

                string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";
                var imageSource = await ds.GetImageFromCacheOrDownload(downloadUrl, fileId);

                proveedor.ImagenProveedor = imageSource;
                _imagenOriginalDeRespaldo = imageSource;

                OnPropertyChanged(nameof(ProveedorSeleccionado));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error cargando imagen: " + ex.Message);
            }
        }

        private void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (!HasChanges) return;

            if (ProveedorSeleccionado == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Seleccionar imagen del proveedor"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    ProveedorSeleccionado.ImagenProveedor = bitmap;
                    _rutaImagenLocal = filePath;

                    HasChanges = true;
                    btnCancelarCambios.Visibility = Visibility.Visible;
                    btnGuardarCambios.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al previsualizar la imagen: " + ex.Message);
                }
            }
        }

        private async void CargarProveedoresDesdeAPI()
        {
            try
            {
                var resultadoAPI = await _apiService.GetAsync<List<MProveedores>>("/api/CProveedores");

                if (resultadoAPI != null)
                {
                    ListaProveedores.Clear();

                    foreach (var prov in resultadoAPI)
                    {
                        ListaProveedores.Add(new ProveedorUI
                        {
                            IdProveedor = prov.idProveedor,
                            Nombre = prov.Nombre ?? "Sin Nombre",
                            Correo = prov.Correo ?? "",
                            Telefono = prov.Telefono ?? "",
                            Direccion = prov.Domicilio ?? "",
                            Estado = prov.Estado == 1 ? "Activo" : "Inactivo",
                            Foto = prov.Foto ?? ""
                        });
                    }

                    AplicarOrdenamiento();
                    AplicarFiltroBusqueda();
                    if (ListaProveedores.Count > 0) lstProveedores.SelectedItem = ListaProveedores[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al descargar los proveedores: " + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarOrdenamiento()
        {
            ICollectionView vista = CollectionViewSource.GetDefaultView(ListaProveedores);
            if (vista != null)
            {
                vista.SortDescriptions.Clear();
                vista.SortDescriptions.Add(new SortDescription("Estado", ListSortDirection.Ascending));
                vista.SortDescriptions.Add(new SortDescription("Nombre", ListSortDirection.Ascending));
                vista.Refresh();
            }
        }

        private void BotonAgregar_Click(object sender, RoutedEventArgs e)
        {
            if (HasChanges)
            {
                var respuesta = MessageBox.Show(
                         "Detectamos que tienes cambios sin guardar.\n\n" +
                         "[Aceptar] = Descartar los cambios y agregar un nuevo proveedor.\n" +
                         "[Cancelar] = Quedarse aquí y continuar editando.",
                         "Cambios sin guardar",
                         MessageBoxButton.OKCancel,
                         MessageBoxImage.Warning);
                if (respuesta == MessageBoxResult.Cancel) return;
            }

            lstProveedores.SelectedItem = null;
            _isNewItem = true;
            _rutaImagenLocal = null;
            ProveedorSeleccionado = new ProveedorUI { Nombre = "", Estado = "Activo", Foto = "" };
            CargarImagenDelProveedor(ProveedorSeleccionado);
            HasChanges = true;
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedorSeleccionado != null)
            {
                _isNewItem = false;
                HasChanges = true;
            }
        }

        private void btnToggleEstado_Click(object sender, RoutedEventArgs e)
        {
            if (!HasChanges) return;
            if (ProveedorSeleccionado != null)
            {
                ProveedorSeleccionado.Estado = (ProveedorSeleccionado.Estado == "Activo") ? "Inactivo" : "Activo";
            }
        }

        private MProveedores PrepararModeloParaApi(ProveedorUI modeloPantalla)
        {
            if (modeloPantalla == null) return null;

            return new MProveedores
            {
                idProveedor = modeloPantalla.IdProveedor,
                Nombre = modeloPantalla.Nombre,
                Correo = modeloPantalla.Correo,
                Telefono = modeloPantalla.Telefono,
                Domicilio = modeloPantalla.Direccion,
                Estado = modeloPantalla.Estado == "Activo" ? 1 : 0,
                Foto = modeloPantalla.Foto,
                FechaRegistro = DateTime.Now
            };
        }

        private async void btnGuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedorSeleccionado == null || string.IsNullOrWhiteSpace(ProveedorSeleccionado.Nombre))
            {
                MessageBox.Show("El nombre del proveedor es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                btnGuardarCambios.IsEnabled = false;

                if (!string.IsNullOrEmpty(_rutaImagenLocal))
                {
                    string fileId = await Controlador.GoogleDriveHelper.UploadFileAsync(_rutaImagenLocal, ID_CARPETA_DRIVE);
                    await GoogleDriveHelper.MakeFilePublicAsync(fileId);

                    ProveedorSeleccionado.Foto = fileId;
                }

                MProveedores datosParaEnviar = PrepararModeloParaApi(ProveedorSeleccionado);
                bool exito = false;
                string mensajeAccion = _isNewItem ? "crear" : "actualizar";

                if (_isNewItem)
                {
                    exito = await _apiService.PostAsync("/api/CProveedores", datosParaEnviar);
                }
                else
                {
                    string endpoint = $"/api/CProveedores/{datosParaEnviar.idProveedor}";
                    exito = await _apiService.PutAsync(endpoint, datosParaEnviar);
                }

                if (exito)
                {
                    MessageBox.Show($"¡Proveedor {mensajeAccion}do correctamente en la base de datos!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    _isNewItem = false;
                    HasChanges = false;
                    _rutaImagenLocal = null;

                    CargarProveedoresDesdeAPI();
                }
                else
                {
                    MessageBox.Show($"La API respondió que no se pudo {mensajeAccion} el proveedor.", "Error de Servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                btnGuardarCambios.IsEnabled = true;
            }
        }

        private void btnCancelarCambios_Click(object sender, RoutedEventArgs e)
        {
            _rutaImagenLocal = null;

            if (_isNewItem)
            {
                ProveedorSeleccionado = null;
                _isNewItem = false;
            }
            else if (_proveedorOriginalEnLista != null)
            {
                ProveedorSeleccionado.Nombre = _proveedorOriginalEnLista.Nombre;
                ProveedorSeleccionado.Correo = _proveedorOriginalEnLista.Correo;
                ProveedorSeleccionado.Telefono = _proveedorOriginalEnLista.Telefono;
                ProveedorSeleccionado.Direccion = _proveedorOriginalEnLista.Direccion;
                ProveedorSeleccionado.Estado = _proveedorOriginalEnLista.Estado;

                // Forzamos la descarga/recarga de la imagen
                ProveedorSeleccionado.Foto = _proveedorOriginalEnLista.Foto;
                CargarImagenDelProveedor(ProveedorSeleccionado);
            }
            HasChanges = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class EstadoToColorConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string colorHex = value?.ToString() == "Activo" ? "#6B9D89" : "#E57373";
            return (System.Windows.Media.SolidColorBrush)new System.Windows.Media.BrushConverter().ConvertFrom(colorHex);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }
}