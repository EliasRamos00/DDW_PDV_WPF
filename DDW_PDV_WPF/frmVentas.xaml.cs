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
        private string qrLeido="";
        private decimal _montoRecibido;
        private decimal _cambio;


        private ObservableCollection<MCategorias> _categorias;

        public decimal Cambio
        {
            get => _montoRecibido-_total;

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
            if(Cambio > 0)
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

        public frmVentas(string currentUser)
        {
            InitializeComponent();
            time();
            // Inicializar ApiService
            _apiService = new ApiService();
            CargarProductos();
            CargarCategorias();
            DataContext = this;
            _usuario = currentUser;

        }

        private async void CargarCategorias()
        {
            var resultado = await _apiService.GetAsync<List<MCategorias>>("api/CCategorias/");

            if (resultado != null)
            {
                Categorias = new ObservableCollection<MCategorias>(resultado); //  Asigna los datos a la propiedad
            }
        }

        public void time ()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Actualizar la fecha y hora al iniciar
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


        private async void CargarProductos()
        {
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("api/CArticulos/productos/inventario");

            if (resultado != null)
            {
                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultado); //  Asigna los datos a la propiedad
            }
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
                        Foto = articuloEscaneado.Foto,
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
                MessageBox.Show("Artículo no encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        Foto = producto.Foto,
                        PrecioVenta = producto.PrecioVenta, // precio
                        Cantidad = 1
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
                    MessageBox.Show("El carrito está vacío.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Construimos el objeto para la API
                var ventaDTO = new
                {
                    venta = new
                    {
                        idVenta = 0,  // Será generado en la BD
                        total = _total ,
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
                    MessageBox.Show("Venta registrada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    ImpresoraTicket.ImprimeTicket(_carritoVenta, _total);
                    _carritoVenta.Clear(); // Limpiar el carrito después de la venta
                    _total = 0;
                    _subTotal = 0;
                    _montoRecibido = 0;
                    CalcularTotalCarro();


                }
                else
                {
                    MessageBox.Show("Error al registrar la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la conexión con la API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

}
