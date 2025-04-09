using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Xml.Serialization;
using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmHistorialModificaciones.xaml
    /// </summary>
    
    
    public partial class frmHistorialModificaciones : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ApiService _apiService;
        private ObservableCollection<HistorialDTO> _listaHistorial;
        private HistorialDTO _historialSeleccionado;
        private string _textoBusqueda;
        private ObservableCollection<HistorialDTO> _todosLosHistoriales;

        public ObservableCollection<HistorialDTO> ListaHistorial
        {
            get => _listaHistorial;
            set
            {
                _listaHistorial = value;
                OnPropertyChanged();
            }
        }

        public   HistorialDTO HistorialSeleccionado
        {
            get => _historialSeleccionado;
            set
            {
                _historialSeleccionado = value;
                OnPropertyChanged();

                if (value != null)
                {
                    // Decodificar los XML cuando se selecciona un item
                    DecodificarDatosHistorial(value);
                }
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                FiltrarHistorial();
            }
        }
        public frmHistorialModificaciones()
        {
            InitializeComponent();
            DataContext = this;
            _apiService = new ApiService();
            CargarDatos();
        }

        private async void CargarDatos()
        {
            try
            {
                var resultado = await _apiService.GetAsync<List<HistorialDTO>>("/api/CHistoriales");

                if (resultado != null)
                {
                    _todosLosHistoriales = new ObservableCollection<HistorialDTO>(resultado);
                    ListaHistorial = new ObservableCollection<HistorialDTO>(resultado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el historial: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarHistorial()
        {
            if (_todosLosHistoriales == null) return;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                ListaHistorial = new ObservableCollection<HistorialDTO>(_todosLosHistoriales);
            }
            else
            {
                var texto = TextoBusqueda.ToLower();
                var resultados = _todosLosHistoriales
                    .Where(h =>
                        (h.idUsuario != null && h.idUsuario.ToLower().Contains(texto)) ||
                        (h.accion != null && h.accion.ToLower().Contains(texto)) ||
                        (h.fechaHora.ToString().Contains(texto)) ||
                        (h.clase != null && h.clase.ToLower().Contains(texto)))
                    .ToList();

                ListaHistorial = new ObservableCollection<HistorialDTO>(resultados);
            }
        }

        private void DecodificarDatosHistorial(HistorialDTO historial)
        {
            try
            {
                // Mostrar información básica
                txtFechaHora.Text = historial.fechaHora.ToString();
                txtUsuario.Text = historial.idUsuario;
                txtTipoModificacion.Text = historial.accion;

                // Decodificar y mostrar datos "antes"
                ArticuloDTO articuloAntes = null;
                if (!string.IsNullOrEmpty(historial.antes))
                {
                    articuloAntes = DeserializarDeXml<ArticuloDTO>(historial.antes);
                    if (articuloAntes != null)
                    {
                        txtAntes.Text = FormatearDatosArticulo(articuloAntes);
                    }
                }
                // Decodificar y mostrar datos "después"
                ArticuloDTO articuloDespues = null;
                if (!string.IsNullOrEmpty(historial.despues))
                {
                    articuloDespues = DeserializarDeXml<ArticuloDTO>(historial.despues);
                    if (articuloDespues != null)
                    {
                        txtDespues.Text = FormatearDatosArticulo(articuloDespues);
                    }
                }
                // Mostrar comparación de cambios
                txtMotivo.Text = CompararCambios(articuloAntes, articuloDespues);

                // Configurar lista de productos (mostrar ambos estados si existen)
                var productos = new List<ArticuloDTO>();
                if (articuloAntes != null) productos.Add(articuloAntes);
                if (articuloDespues != null) productos.Add(articuloDespues);
                lstProductos.ItemsSource = productos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al decodificar historial: {ex.Message}");
                txtMotivo.Text = "Error al mostrar los cambios";
            }
        }


        private string FormatearDatosArticulo(ArticuloDTO articulo)
        {
            if (articulo == null) return "N/A";
            return $@"ID: {articulo.idArticulo}
            Descripción: {articulo.Descripcion}
            Categoría: {articulo.IdCategoria}
            Color: {articulo.Color}
            Tamaño: {articulo.Tamanio}
            Stock: {articulo.Stock}
            Mínimo: {articulo.Min}
            Máximo: {articulo.Max}
            Precio Venta: {articulo.PrecioVenta:C}
            Precio Compra: {articulo.PrecioCompra:C}
            Código: {articulo.CodigoBarras}";
        }

        private string CompararCambios(ArticuloDTO antes, ArticuloDTO despues)
        {
            if (antes == null && despues == null) return "No hay datos para comparar";

            if (antes == null) return "CREACIÓN: \n" + FormatearDatosArticulo(despues);
            if (despues == null) return "ELIMINACIÓN: \n" + FormatearDatosArticulo(antes);

            var cambios = new StringBuilder("CAMBIOS REALIZADOS:\n");

            if (antes.Descripcion != despues.Descripcion)
                cambios.AppendLine($"• Descripción: {antes.Descripcion} → {despues.Descripcion}");

            if (antes.IdCategoria != despues.IdCategoria)
                cambios.AppendLine($"• Categoría: {antes.IdCategoria} → {despues.IdCategoria}");

            if (antes.Color != despues.Color)
                cambios.AppendLine($"• Color: {antes.Color} → {despues.Color}");

            if (antes.Tamanio != despues.Tamanio)
                cambios.AppendLine($"• Tamaño: {antes.Tamanio} → {despues.Tamanio}");

            if (antes.Stock != despues.Stock)
                cambios.AppendLine($"• Stock: {antes.Stock} → {despues.Stock}");

            if (antes.Min != despues.Min)
                cambios.AppendLine($"• Mínimo: {antes.Min} → {despues.Min}");

            if (antes.Max != despues.Max)
                cambios.AppendLine($"• Máximo: {antes.Max} → {despues.Max}");

            if (antes.PrecioVenta != despues.PrecioVenta)
                cambios.AppendLine($"• Precio Venta: {antes.PrecioVenta:C} → {despues.PrecioVenta:C}");

            if (antes.PrecioCompra != despues.PrecioCompra)
                cambios.AppendLine($"• Precio Compra: {antes.PrecioCompra:C} → {despues.PrecioCompra:C}");

            if (antes.CodigoBarras != despues.CodigoBarras)
                cambios.AppendLine($"• Código: {antes.CodigoBarras} → {despues.CodigoBarras}");

            // Si no hay cambios pero es una actualización
            if (cambios.Length == "CAMBIOS REALIZADOS:\n".Length)
                return "REGISTRO ACTUALIZADO (sin cambios detectados)";

            return cambios.ToString();
        }


        private T DeserializarDeXml<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml)) return null;

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(xml))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al deserializar XML: {ex.Message}");
                return null;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is HistorialDTO historial)
            {
               
                  DecodificarDatosHistorial(historial);
               
                HistorialSeleccionado = historial;

                // Decodificar los XML para el panel de detalles
                var antes = DeserializarDeXml<ArticuloDTO>(historial.antes);
                var despues = DeserializarDeXml<ArticuloDTO>(historial.despues);

                // Actualizar controles del panel derecho
                txtFechaHora.Text = historial.fechaHora.ToString();
                txtUsuario.Text = historial.idUsuario;
                txtTipoModificacion.Text = historial.accion;

                // Configurar lista de productos
                lstProductos.ItemsSource = new List<ArticuloDTO>
        {
            new ArticuloDTO { Descripcion = "ANTES", Stock = antes?.Stock ?? 0 },
            new ArticuloDTO { Descripcion = "DESPUÉS", Stock = despues?.Stock ?? 0 }
        };

               
            }
        }



    }
}
