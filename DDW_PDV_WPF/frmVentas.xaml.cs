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
using System.Diagnostics;

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
        private ArticuloDTO articuloEnEdicion;
        private bool _puedeEditarTotal;
        private decimal _totalCarrito;
        private Dictionary<int, decimal> _preciosModificados = new Dictionary<int, decimal>();
        private bool huboCambio=false;
        private CarritoViewModel _carritoSeleccionado;
        private static ObservableCollection<ArticuloDTO> _productosEnMemoria;
        private static bool _productosCargados = false;
        private int _paginaActual = 1;
        private int _itemsPorPagina = 30;
        private int _totalPaginas = 1;


        private ObservableCollection<CarritoViewModel> _carritos = new ObservableCollection<CarritoViewModel>()
        {
            new CarritoViewModel { Nombre = "Carrito 1", Articulos = new ObservableCollection<ArticuloDTO>() },
            new CarritoViewModel { Nombre = "Carrito 2", Articulos = new ObservableCollection<ArticuloDTO>() },
            new CarritoViewModel { Nombre = "Carrito 3", Articulos = new ObservableCollection<ArticuloDTO>() }
        };

        public CarritoViewModel CarritoSeleccionado
        {
            get => _carritoSeleccionado;
            set
            {
                if (_carritoSeleccionado != value)
                {
                    _carritoSeleccionado = value;
                    OnPropertyChanged(nameof(CarritoSeleccionado));

                    // Volver a calcular totales cuando cambia de carrito
                    CalcularTotalCarro();
                }
            }
        }

        public ObservableCollection<CarritoViewModel> Carritos
        {
            get => _carritos;
            set
            {
                _carritos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Carritos)));
            }
        }


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


        public int PaginaActual
        {
            get => _paginaActual;
            set
            {
                if (_paginaActual != value)
                {
                    _paginaActual = value;
                    OnPropertyChanged(nameof(PaginaActual));
                }
            }
        }

        public int TotalPaginas
        {
            get => _totalPaginas;
            set
            {
                if (_totalPaginas != value)
                {
                    _totalPaginas = value;
                    OnPropertyChanged(nameof(TotalPaginas));
                }
            }
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
       

        public decimal TotalCarrito
        {
            get => _totalCarrito;
            set
            {
                if (_totalCarrito != value)
                {
                    _totalCarrito = value;
                    OnPropertyChanged(nameof(TotalCarrito));
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
            set
            {
                if (_total != value)
                {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(Cambio));
                }
            }
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

       
        public bool PuedeEditarTotal
        {
            get => _puedeEditarTotal;
            set
            {
                if (_puedeEditarTotal != value)
                {
                    _puedeEditarTotal = value;
                    if (_puedeEditarTotal == false)
                    {
                        CalcularTotalCarro();
                    }
                    OnPropertyChanged(nameof(PuedeEditarTotal));
                }
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
            {
                InitializeComponent();
                time();
                // Inicializar ApiService
                _apiService = new ApiService();

                if (!_productosCargados)
                    CargarProductos();
                else
                    ListaArticulos = new ObservableCollection<ArticuloDTO>(_productosEnMemoria);

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
        }

        private async void CargarCategorias()
        {
            try
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
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al cargar categorías: " + ex.Message);
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


        private void BtnAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (PaginaActual > 1) 
            {
                PaginaActual--;   
                FiltrarProductos();
            }
        }

        private void BtnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (PaginaActual < TotalPaginas) 
            {
                PaginaActual++;              
                FiltrarProductos();
            }
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
            if (CarritoSeleccionado == null) return;

            decimal aux = 0;

            foreach (var dto in CarritoSeleccionado.Articulos)
            {
                decimal total = dto.PrecioDescuento > 0
                    ? dto.PrecioDescuento * dto.Cantidad
                    : dto.PrecioVenta * dto.Cantidad;

                aux += total;
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
            txtFecha.Text = now.ToString("dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"));

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
                if (CarritoSeleccionado != null)
                {
                    CarritoSeleccionado.Articulos.Clear();
                    CalcularTotalCarro(); // Asegúrate de que este método calcule para CarritoSeleccionado.Articulos
                }
            }
        }

        

        private object itemActual; // Hay que ponerlo en donde corresponde.

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
                // Actualiza la cantidad
                itemActual.GetType().GetProperty("Cantidad")?.SetValue(itemActual, cantidad);

                // Si cantidad es 0, eliminar del carrito seleccionado
                if (cantidad == 0)
                {
                    CarritoSeleccionado.Articulos.Remove((ArticuloDTO)itemActual);
                }

                // Recalcula totales con carrito actual
                CalcularTotalCarro();
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
            if (_productosCargados) return;

            try
            {
                var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("api/CArticulos/productos/inventario");

                if (resultado != null)
                {
                    _productosOriginales = new ObservableCollection<ArticuloDTO>(resultado);
                    _paginaActual = 1;

                    // Carga imágenes (optimizado)
                    foreach (var article in _productosOriginales)
                    {
                        if (!string.IsNullOrEmpty(article.Foto) && article.ImagenProducto == null)
                        {
                            try
                            {
                                string fileId = article.Foto;
                                string downloadUrl = $"https://drive.google.com/uc?export=download&id={fileId}";
                                var image = await ds.GetImageFromCacheOrDownload(downloadUrl, fileId);
                                if (image != null && image.CanFreeze)
                                    image.Freeze();

                                article.ImagenProducto = image;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error imagen: {ex.Message}");
                            }
                        }
                    }

                    _productosCargados = true;
                    FiltrarProductos();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
        }


        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            _productosCargados = false;
            _productosEnMemoria = null;
            CargarProductos();
        }

        private void FiltrarProductos()
        {
            if (_productosOriginales == null) return;

            var productosFiltrados = _productosOriginales.AsEnumerable();

            if (CategoriaSeleccionada != null && CategoriaSeleccionada.idCategoria != 0)
            {
                productosFiltrados = productosFiltrados.Where(p => p.idCategoria == CategoriaSeleccionada.idCategoria);
            }

            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                string texto = TextoBusqueda.ToLower();
                productosFiltrados = productosFiltrados.Where(p =>
                    p.Descripcion?.ToLower().Contains(texto) == true ||
                    (!string.IsNullOrEmpty(p.Color) && p.Color.ToLower().Contains(texto)) ||
                    p.PrecioVenta.ToString().Contains(texto)
                );
            }

            int totalItems = productosFiltrados.Count();

            TotalPaginas = (int)Math.Ceiling((double)totalItems / _itemsPorPagina);

            if (PaginaActual < 1) PaginaActual = 1;
            if (PaginaActual > TotalPaginas) PaginaActual = TotalPaginas;

            var paginados = productosFiltrados
                .Skip((PaginaActual - 1) * _itemsPorPagina)
                .Take(_itemsPorPagina)
                .ToList();
            ListaArticulos = new ObservableCollection<ArticuloDTO>(paginados);
        }





        private void IncrementarCantidad(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticuloDTO producto)
            {
                producto.Cantidad++;
                producto.PrecioDescuento = 0;

                //if (_preciosModificados.ContainsKey(producto.idArticulo))
                //{
                //    // Si tiene precio modificado, eliminar para restablecer original
                //    _preciosModificados.Remove(producto.idArticulo);
                //    producto.TotalCarrito = producto.Cantidad * producto.PrecioVenta;
                //}
                //else
                //{
                //}

                producto.TotalCarrito = producto.Cantidad * producto.PrecioVenta;

                producto.AlertaDescuento = producto.Cantidad >= 6 ? Visibility.Visible : Visibility.Hidden;
            }

            CalcularTotalCarro();
            popupEditarPrecio.IsOpen = false;
        }




        private void DecrementarCantidad(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticuloDTO producto)
            {
                if (producto.Cantidad > 0)
                {
                    producto.Cantidad--;
                    producto.PrecioDescuento = 0;

                    if (producto.Cantidad >= 6)
                    {
                        producto.AlertaDescuento = Visibility.Visible;
                    }
                    else
                    {
                        producto.AlertaDescuento = Visibility.Hidden;
                        //if (_preciosModificados.ContainsKey(producto.idArticulo))
                        //{
                        //    _preciosModificados.Remove(producto.idArticulo);
                        //}
                    }

                    producto.TotalCarrito = producto.Cantidad * producto.PrecioVenta;

                    if (producto.Cantidad == 0)
                    {
                        CarritoSeleccionado.Articulos.Remove(producto);  // O _carritoVenta.Remove(producto); según cómo estés manejando la colección
                    }
                }

                CalcularTotalCarro();
            }
            popupEditarPrecio.IsOpen = false;
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

        private void AbrirPopup_Click(object sender, RoutedEventArgs e)
        {
            popupEditarPrecio.IsOpen = true;
        }
        private void ScannArticulo(string CodigoArticulo)
        {
            try
            {
                var articuloEscaneado = _listaArticulos.FirstOrDefault(a => a.CodigoBarras == CodigoArticulo);

                if (articuloEscaneado != null)
                {
                    var articuloEnCarrito = CarritoSeleccionado?.Articulos.FirstOrDefault(item => item.idArticulo == articuloEscaneado.idArticulo);

                    if (articuloEnCarrito != null)
                    {
                        articuloEnCarrito.Cantidad++;
                        articuloEnCarrito.TotalCarrito = articuloEnCarrito.Cantidad * articuloEnCarrito.PrecioVenta;

                        if (articuloEnCarrito.Cantidad >= 6)
                        {
                            articuloEnCarrito.AlertaDescuento = Visibility.Visible;
                            popupEditarPrecio.IsOpen = true;
                        }
                    }
                    else
                    {
                        ArticuloDTO nuevoArticulo = new ArticuloDTO
                        {
                            idArticulo = articuloEscaneado.idArticulo,
                            Descripcion = articuloEscaneado.Descripcion,
                            ImagenProducto = articuloEscaneado.ImagenProducto,
                            PrecioVenta = articuloEscaneado.PrecioVenta,
                            Cantidad = 1
                        };

                        CarritoSeleccionado?.Articulos.Add(nuevoArticulo);
                        nuevoArticulo.TotalCarrito = nuevoArticulo.Cantidad * nuevoArticulo.PrecioVenta;
                    }

                    OnPropertyChanged(nameof(CarritoSeleccionado));
                    CalcularTotalCarro();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al escanear artículo: " + ex.Message);
            }
        }

        private void AceptarPopup(object sender, RoutedEventArgs e)
        {
            if (articuloEnEdicion != null && decimal.TryParse(txtNuevoPrecio.Text, out decimal nuevoPrecio))
            {
                articuloEnEdicion.PrecioDescuento = nuevoPrecio ;
                articuloEnEdicion.TotalCarrito = nuevoPrecio * articuloEnEdicion.Cantidad;

                //// Guardar el nuevo precio modificado en el diccionario
                //_preciosModificados[articuloEnEdicion.idArticulo] = nuevoPrecio;

                //// Actualizar el TotalCarrito basado en la nueva cantidad * nuevo precio
                //articuloEnEdicion.TotalCarrito = articuloEnEdicion.Cantidad * nuevoPrecio;

                //// Actualizar la UI y recalcular totales del carrito activo
                ///
                articuloEnEdicion.AlertaDescuento = Visibility.Hidden;
                CalcularTotalCarro();
                OnPropertyChanged(nameof(CarritoSeleccionado)); // Para refrescar UI si es necesario
            }

            popupEditarPrecio.IsOpen = false;
        }





        private void CancelarPopup(object sender, RoutedEventArgs e)
        {
            popupEditarPrecio.IsOpen = false;
            articuloEnEdicion = null;
        }



        private void ClickProducto(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ArticuloDTO producto && CarritoSeleccionado != null)
            {
                var articuloExistente = CarritoSeleccionado.Articulos
                    .FirstOrDefault(a => a.idArticulo == producto.idArticulo);

                if (articuloExistente != null)
                {
                    articuloExistente.Cantidad++;
                    articuloExistente.TotalCarrito = articuloExistente.Cantidad * articuloExistente.PrecioVenta;

                    if (articuloExistente.Cantidad >= 6)
                    {
                        articuloExistente.AlertaDescuento = Visibility.Visible;
                    }
                }
                else
                {
                    var nuevoArticulo = new ArticuloDTO
                    {
                        idArticulo = producto.idArticulo,
                        Descripcion = producto.Descripcion,
                        ImagenProducto = producto.ImagenProducto,
                        PrecioVenta = producto.PrecioVenta,
                        Cantidad = 1,
                        Color = producto.Color,
                        TotalCarrito = producto.PrecioVenta,
                        AlertaDescuento = Visibility.Collapsed
                    };

                    CarritoSeleccionado.Articulos.Add(nuevoArticulo);
                }
                OnPropertyChanged(nameof(Carritos));

                CalcularTotalCarro();
            }
        }



        private async void CerrarVenta(object sender, RoutedEventArgs e)
        {
            await Task.Delay(200);

            if (CarritoSeleccionado == null || CarritoSeleccionado.Articulos == null || !CarritoSeleccionado.Articulos.Any())
            {
                Growl.Info(new GrowlInfo
                {
                    Message = "El carrito está vacío.",
                    WaitTime = 3,
                    Type = InfoType.Warning
                });
                return;
            }

            var ventaDTO = new
            {
                venta = new
                {
                    idVenta = 0,
                    total = _total,
                    fechahora = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    vendedor = Properties.Settings.Default.Usuario,
                    tieneFactura = 0,
                    idSucursal = Convert.ToInt16(Properties.Settings.Default.Sucursal),
                    idCaja = Properties.Settings.Default.Caja
                },
                detalle = CarritoSeleccionado.Articulos.Select(a =>
                {
                    decimal precioUnitario = a.PrecioDescuento != 0 ? a.PrecioDescuento : a.PrecioVenta;

                    return new
                    {
                        idArticulo = a.idArticulo,
                        idVenta = 0,
                        precioVenta = precioUnitario,
                        cantidad = a.Cantidad
                    };
                }).ToList()
            };

            try
            {
                var resultado = await _apiService.PostAsync("api/CVentas/crear", ventaDTO);

                if (resultado)
                {
                    try
                    {
                        ImpresoraTicket.ImprimeTicket(CarritoSeleccionado.Articulos, _total);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show("Error al imprimir ticket: " + ex.Message);
                    }

                    CarritoSeleccionado.Articulos.Clear();
                    _total = 0;
                    _subTotal = 0;
                    _montoRecibido = 0;
                    _preciosModificados = new Dictionary<int, decimal>();
                    CalcularTotalCarro();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Error al registrar la venta.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al cerrar la venta: " + ex.Message);
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

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ArticuloDTO producto)
            {
                if(producto.Cantidad >= 6)
                {
                    articuloEnEdicion = producto;
                    txtNuevoPrecio.Text = producto.PrecioVenta.ToString("0.##");
                    popupEditarPrecio.IsOpen = true;
                }
               
            }
        }
    }


    public class CarritoViewModel
    {
        public string Nombre { get; set; }
        public ObservableCollection<ArticuloDTO> Articulos { get; set; }
    }

    public class DecimalToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue.ToString("C2", culture);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (decimal.TryParse(strValue, NumberStyles.Currency, culture, out var result))
                    return result;
            }

            return Binding.DoNothing;
        }
    }


}
