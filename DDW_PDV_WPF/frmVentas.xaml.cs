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

        private ObservableCollection<MCategorias> _categorias;


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

        public frmVentas()
        {
            InitializeComponent();
            time();
            // Inicializar ApiService
            _apiService = new ApiService();
            CargarProductos();
            CargarCategorias();
            DataContext = this;
            

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


        private void CalcularTotalCarro()
        {
            decimal aux = 0;
            foreach (ArticuloDTO dto in _carritoVenta)
            {
                aux = aux + (dto.PrecioVenta * dto.Cantidad);
            }
            _total = aux;
            _subTotal = aux;
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(SubTotal));

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
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("api/CArticulos/");

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
                        PrecioVenta = 30, // precio
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
        
    }
    
}
