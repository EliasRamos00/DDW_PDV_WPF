using DDW_PDV_WPF.Controlador;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DDW_PDV_WPF.Modelo;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Dropbox.Api.TeamCommon;
using HandyControl.Data;
using HandyControl.Controls;
using System.Windows.Threading;

namespace DDW_PDV_WPF
{


    /// <summary>
    /// Lógica de interacción para frmVentas.xaml
    /// </summary>
    public partial class frmVentas : Page, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private DispatcherTimer _timer;
        private readonly ApiService _apiService;
        private ObservableCollection<ArticuloDTO> _listaArticulos;
        private ObservableCollection<ArticuloDTO> _carritoVenta = new ObservableCollection<ArticuloDTO>();
        private int _cantidad;
        private decimal _subTotal;
        private decimal _total;
        private string _usuario;
        private string qrLeido = "";
        private decimal _montoRecibido;
        private decimal _cambio;
        private ObservableCollection<MCategorias> _categorias;
        private string _textoBusqueda;
        private MCategorias _categoriaSeleccionada;
        private Button _botonCategoriaSeleccionado;
        private Button _botonTodos;
        GoogleDriveHelper ds;
        private DispatcherTimer _debounceTimer;



        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                if (_textoBusqueda != value)
                {
                    _textoBusqueda = value;
                    OnPropertyChanged(nameof(TextoBusqueda));

                    // Reiniciar el debounce timer
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
            }
        }


        public MCategorias CategoriaSeleccionada
        {
            get => _categoriaSeleccionada;
            set
            {
                if (_categoriaSeleccionada != value)
                {
                    _categoriaSeleccionada = value;
                    OnPropertyChanged(nameof(CategoriaSeleccionada));
                    FiltrarProductos();
                }
            }
        }

        private ObservableCollection<ArticuloDTO> _productosOriginales; 
        public decimal Cambio
        {
            get => _montoRecibido - _total;

        }


        public decimal MontoRecibido
        {
            get => _montoRecibido;
            set
            {
                if (_montoRecibido != value)
                {
                    _montoRecibido = value;
                    _cambio = _montoRecibido - _total;
                    CambiarColorCambio();
                    OnPropertyChanged(nameof(Cambio));
                    OnPropertyChanged(nameof(MontoRecibido));
                }
            }

        }

        private void CambiarColorCambio()
        {
            if (Cambio > 0)
            {
                txtCambio.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (Cambio < 0)
            {
                txtCambio.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                txtCambio.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        public decimal SubTotal
        {
            get => _subTotal;

        }

        public decimal Total
        {
            get => _total;
        }



        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad != value)
                {
                    _cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                }
            }
        }


        public ObservableCollection<ArticuloDTO> ListaArticulos
        {
            get { return _listaArticulos; }
            set
            {
                _listaArticulos = value;
                OnPropertyChanged(nameof(ListaArticulos)); // 
            }
        }

        public ObservableCollection<MCategorias> Categorias
        {
            get { return _categorias; }
            set
            {
                _categorias = value;
                OnPropertyChanged(nameof(Categorias)); // 
            }
        }


        public ObservableCollection<ArticuloDTO> CarritoVenta
        {
            get { return _carritoVenta; }
            set
            {
                _carritoVenta = value;
                OnPropertyChanged(nameof(CarritoVenta)); // 
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// CONSTRUCTOR DE LA CLASE FRMVENTAS
        /// </summary>
        /// <param name="currentUser"></param>
        public frmVentas(string currentUser, GoogleDriveHelper ds)
        {
            InitializeComponent();
            time();
            // Inicializar ApiService
            _apiService = new ApiService();
            CargarProductos();
            CargarCategorias();

            DataContext = this;

            _usuario = currentUser;

            if (btnTodos != null)
            {
                _botonTodos = btnTodos;
                _botonCategoriaSeleccionado = btnTodos;
                btnTodos.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5C1DC"));
                btnTodos.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
            }
            // se cargan las imagenes
            this.ds = ds;
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop(); // Detener el timer para evitar múltiples ejecuciones
                FiltrarProductos(); // Ejecutar el filtro
            };
        }

        private async void CargarCategorias()
        {
            var resultado = await _apiService.GetAsync<List<MCategorias>>("api/CCategorias/");

            if (resultado != null)
            {
                Categorias = new ObservableCollection<MCategorias>(resultado);


                if (_botonCategoriaSeleccionado != null)
                {
                    _botonCategoriaSeleccionado.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5C1DC"));
                    _botonCategoriaSeleccionado.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                }
                else if (btnTodos != null)
                {

                    _botonCategoriaSeleccionado = btnTodos;
                    btnTodos.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5C1DC"));
                    btnTodos.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                }
            }
        }

        public void time()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateDateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Actualizar la fecha y hora en cada tick del timer
            UpdateDateTime();
        }


        private void Window_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

            if (e.Text == "\r") // Si detecta Enter, significa que el código está completo
            {

                string CodigoArticulo = qrLeido;
                Console.WriteLine(CodigoArticulo);
                //ProcesarCodigo(codigoActual);
                ScannArticulo(CodigoArticulo);
                qrLeido = ""; // Reinicia el código
            }
            else
            {
                qrLeido += e.Text; // Agrega cada carácter leído
            }
        }

        private void CalcularTotalCarro()
        {
            decimal aux = 0;
            foreach (ArticuloDTO dto in _carritoVenta)
            {
                aux = aux + (dto.PrecioVenta * dto.Cantidad);
            }
            _total = aux;
            _subTotal = aux;
            CambiarColorCambio();
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(SubTotal));
            OnPropertyChanged(nameof(Cambio));


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


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            tb?.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
        }
        private void LimpiarCarrito_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("¿Estás seguro de que deseas limpiar el carrito?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                CarritoVenta.Clear();
                CalcularTotalCarro();
            }
        }
        private object itemActual;

        private void CantidadTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext != null)
            {
                itemActual = tb.DataContext;
                PopupCantidadBox.Text = (itemActual.GetType().GetProperty("Cantidad")?.GetValue(itemActual) ?? "1").ToString();
                CantidadPopup.IsOpen = true;


            }
        }

        private void AceptarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (itemActual != null && int.TryParse(PopupCantidadBox.Text, out int cantidad))
            {
                itemActual.GetType().GetProperty("Cantidad")?.SetValue(itemActual, cantidad);
                CalcularTotalCarro();

                if (Convert.ToUInt32(itemActual.GetType().GetProperty("Cantidad")?.GetValue(itemActual)) == 0)
                {
                    _carritoVenta.Remove((ArticuloDTO)itemActual);
                }
            }

            CantidadPopup.IsOpen = false;
        }


        private void CerrarPopup_Click(object sender, RoutedEventArgs e)
        {
            CantidadPopup.IsOpen = false;
        }

        private void chkBDescuentoClick(object sender, RoutedEventArgs e)
        {
            var isChecked = (sender as CheckBox)?.IsChecked ?? false;

            txtBTotal.IsEnabled = !isChecked;
            txtBTotal.Background = isChecked ? Brushes.White : Brushes.Transparent;
        }

        private void PopupCantidadBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
           
            e.Handled = !int.TryParse(e.Text, out _);
        }
        private void PopupCantidadBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
        private async void CargarProductos()
        {
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("api/CArticulos/productos/inventario");

            if (resultado != null)
            {
                _productosOriginales = new ObservableCollection<ArticuloDTO>(resultado);
                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultado);


                // Asignar imágenes a los artículos
                foreach (var article in ListaArticulos)
                {
                    if (article.Foto == "" || article.Foto.Equals(DBNull.Value))
                    {
                        continue;
                    }

                    //COMO LAS IMAGENES SON PUBLICAS Y DE ACCESO LIBRE, LITERALMENTE NO SE NECESITA NINGUN SECETRO PARA CONSUMIRLAS.
                    // Suponemos que cada artículo tiene un "ImageId" que corresponde al ID de Google Drive de la imagen
                    string fileId = article.Foto;  // ID del archivo de Google Drive
                    string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";
                    var imageSource = await ds.GetImageFromCacheOrDownload(downloadUrl, fileId); // GUARDA EN CACHE SUPER IMPORTANTE
                    article.ImagenProducto = imageSource; // Asignamos aquí la imagen lista


                }

            }
        }

        private void FiltrarProductos()
        {
            if (_productosOriginales == null) return;

            var productosFiltrados = _productosOriginales.AsEnumerable();

            // Filtro por categoría
            if (CategoriaSeleccionada != null && CategoriaSeleccionada.idCategoria != 0)
            {
                productosFiltrados = productosFiltrados.Where(p => p.idCategoria == CategoriaSeleccionada.idCategoria);
            }

            // Filtro por texto de búsqueda
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                productosFiltrados = productosFiltrados.Where(p =>
            // Búsqueda por descripción
            p.Descripcion.ToLower().Contains(TextoBusqueda) ||


            // Búsqueda por color (si existe la propiedad)
            (!string.IsNullOrEmpty(p.Color) && p.Color.ToLower().Contains(TextoBusqueda)) ||

            // Búsqueda por precio (convertido a string)
            p.PrecioVenta.ToString().Contains(TextoBusqueda) 
        );
            }

            ListaArticulos = new ObservableCollection<ArticuloDTO>(productosFiltrados);
        }

           
        
        private void IncrementarCantidad(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticuloDTO producto)
            {
                producto.Cantidad++;
                producto.TotalCarrito = producto.Cantidad * producto.PrecioVenta;
            }
            CalcularTotalCarro();


        }

        private void DecrementarCantidad(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn && btn.DataContext is ArticuloDTO producto)
            {

                if (producto.Cantidad > 0)
                {
                    producto.Cantidad--;
                    producto.TotalCarrito = producto.Cantidad * producto.PrecioVenta;


                }
                CalcularTotalCarro();

                if (producto.Cantidad == 0)
                {
                    _carritoVenta.Remove(producto);
                }

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el botón que se hizo clic
            Button clickedButton = sender as Button;

            // Restablecer el estilo de todos los botones
            foreach (var child in ((StackPanel)clickedButton.Parent).Children)
            {
                if (child is Button button)
                {
                    button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B"));
                    button.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            // Aplicar el estilo al botón seleccionado
            clickedButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ScannArticulo(string CodigoArticulo)
        {
            // Buscar el artículo en la lista de productos disponibles
            var articuloEscaneado = _listaArticulos.FirstOrDefault(a => a.CodigoBarras == CodigoArticulo);

            if (articuloEscaneado != null)
            {
                // Buscar si el artículo ya está en el carrito de ventas
                var articuloEnCarrito = _carritoVenta.FirstOrDefault(item => item.idArticulo == articuloEscaneado.idArticulo);

                if (articuloEnCarrito != null)
                {
                    // Si el artículo ya existe en el carrito, incrementar cantidad
                    articuloEnCarrito.Cantidad++;
                    articuloEnCarrito.TotalCarrito = articuloEnCarrito.Cantidad * articuloEnCarrito.PrecioVenta;
                }
                else
                {
                    // Si el artículo no está en el carrito, agregarlo como nuevo
                    ArticuloDTO nuevoArticulo = new ArticuloDTO
                    {
                        idArticulo = articuloEscaneado.idArticulo,
                        Descripcion = articuloEscaneado.Descripcion,
                        ImagenProducto = articuloEscaneado.ImagenProducto,
                        PrecioVenta = articuloEscaneado.PrecioVenta,
                        Cantidad = 1
                    };

                    _carritoVenta.Add(nuevoArticulo);
                    nuevoArticulo.TotalCarrito = nuevoArticulo.Cantidad * nuevoArticulo.PrecioVenta;
                }

                // Notificar cambios en el carrito y recalcular el total
                OnPropertyChanged(nameof(CarritoVenta));
                CalcularTotalCarro();
            }
            else
            {
                //MessageBox.Show("Artículo no encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ClickProducto(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticuloDTO producto)
            {

                // Verificar si el artículo ya existe en el carrito
                var articuloExistente = _carritoVenta.FirstOrDefault(item => item.idArticulo == producto.idArticulo);

                if (articuloExistente != null)
                {
                    // Si el artículo ya existe, incrementamos su cantidad
                    articuloExistente.Cantidad++;
                    articuloExistente.TotalCarrito = articuloExistente.Cantidad * articuloExistente.PrecioVenta;

                }
                else
                {
                    // Si el artículo no existe, creamos un nuevo objeto y lo agregamos al carrito
                    ArticuloDTO nuevoArticulo = new ArticuloDTO
                    {
                        idArticulo = producto.idArticulo, // Asumiendo que el producto tiene un ID
                        Descripcion = producto.Descripcion,
                        ImagenProducto = producto.ImagenProducto,
                        PrecioVenta = producto.PrecioVenta, // precio
                        Cantidad = 1,
                        Color=producto.Color
                    };

                    // Agregar el nuevo artículo al carrito de ventas
                    _carritoVenta.Add(nuevoArticulo);
                    nuevoArticulo.TotalCarrito = nuevoArticulo.Cantidad * nuevoArticulo.PrecioVenta;
                }

                // Notificar que el carrito ha cambiado 
                OnPropertyChanged(nameof(CarritoVenta));
                CalcularTotalCarro();
            }
        }

        private async void CerrarVenta(object sender, RoutedEventArgs e)
        {
            await Task.Delay(200); // Espera que termine la animación

            if (_carritoVenta == null)
            {
                //MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                Growl.Info(new GrowlInfo
                {
                    Message = "Proceso completado correctamente.",
                    WaitTime = 3,  // Tiempo en segundos
                    Type = InfoType.Success // Acceso correcto: estático
                });

                return;
            }

            // Construimos el objeto para la API
            var ventaDTO = new
            {
                venta = new
                {
                    idVenta = 0,  // Será generado en la BD
                    total = _total,
                    fechahora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    vendedor = Properties.Settings.Default.Usuario, // Aquí puedes usar un ID real del vendedor
                    tieneFactura = 0, // O puedes hacer que el usuario lo seleccione
                    idSucursal = 1, // Modifica según la sucursal correspondiente
                    idCaja = Properties.Settings.Default.Caja
                },
                detalle = _carritoVenta.Select(a => new
                {
                    idArticulo = a.idArticulo,
                    idVenta = 0, // Se actualizará en la BD
                    precioVenta = a.PrecioVenta,
                    cantidad = a.Cantidad
                }).ToList()
            };

            try
            {
                var resultado = await _apiService.PostAsync("api/CVentas/crear", ventaDTO);

                if (resultado)
                {
                    //MessageBox.Show("Venta registrada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    ImpresoraTicket.ImprimeTicket(_carritoVenta, _total);
                    _carritoVenta.Clear(); // Limpiar el carrito después de la venta
                    _total = 0;
                    _subTotal = 0;
                    _montoRecibido = 0;
                    CalcularTotalCarro();


                }
                else
                {
                    //MessageBox.Show("Error al registrar la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error en la conexión con la API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CerrarVenta(sender, e);
                e.Handled = true; // Esto evita que el Enter haga otras acciones (como hacer un sonido)
                MontoRecibido = 0;
                OnPropertyChanged(nameof(MontoRecibido));
            }
        }
        private void FiltrarPorCategoria(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                // Quitar el resaltado del botón anterior
                if (_botonCategoriaSeleccionado != null)
                {
                    _botonCategoriaSeleccionado.Background = new SolidColorBrush(Colors.White);
                    _botonCategoriaSeleccionado.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                }

                // Poner el resaltado al nuevo botón
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5C1DC"));
                button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000"));
                _botonCategoriaSeleccionado = button;

                int categoriaId = int.Parse(button.Tag.ToString());

                // Buscar la categoría correspondiente
                var categoria = categoriaId == 0
                    ? new MCategorias { idCategoria = 0, Nombre = "TODOS" }
                    : Categorias.FirstOrDefault(c => c.idCategoria == categoriaId);

                CategoriaSeleccionada = categoria;
            }
        }

        private void PopupCantidadBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

       
    }

}
