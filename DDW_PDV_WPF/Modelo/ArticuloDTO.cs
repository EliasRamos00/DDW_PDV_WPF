using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DDW_PDV_WPF.Modelo
{
    [Serializable]
    [XmlRoot("Articulo")]
    public class ArticuloDTO : INotifyPropertyChanged
    {
        private int _cantidad;
        private decimal _totalCarrito;
        private System.Windows.Visibility _alertaDescuento = System.Windows.Visibility.Hidden;
        private bool _totalManual = false;

        [XmlElement("idArticulo")]
        public int idArticulo { get; set; }

        [XmlElement("Foto")]
        public string Foto { get; set; }

        [XmlElement("Color")]
        public string Color { get; set; }

        [XmlElement("Descripcion")]
        public string Descripcion { get; set; }

        [XmlElement("Tamanio")]
        public string Tamanio { get; set; }

        [XmlElement("CodigoBarras")]
        public string CodigoBarras { get; set; }

        [XmlElement("idCategoria")]
        public int idCategoria { get; set; }

        // INVENTARIO
        [XmlElement("idInventario")]
        public int idInventario { get; set; }

        [XmlElement("Stock")]
        public int? Stock { get; set; }

        [XmlElement("Min")]
        public int? Min { get; set; }

        [XmlElement("Max")]
        public int? Max { get; set; }

        [XmlElement("PrecioVenta")]
        public decimal PrecioVenta { get; set; }

        [XmlElement("PrecioCompra")]
        public decimal? PrecioCompra { get; set; }

        [XmlIgnore] // No serializar esta propiedad para el historial
        public decimal Precio { get; set; }

        [XmlIgnore] // No serializar esta propiedad para el historial
        public System.Windows.Visibility AlertaDescuento
        {
            get => _alertaDescuento;
            set
            {
                if (_alertaDescuento != value)
                {
                    _alertaDescuento = value;
                    OnPropertyChanged(nameof(AlertaDescuento)); // Esto es esencial
                }
            }
        }

        [XmlIgnore] // No serializar esta propiedad para el historial
        [JsonIgnore] // Si usas Newtonsoft.Json para evitar que esta se serialice
        public ImageSource ImagenProducto { get; set; } // Esta se usará en el binding


        [XmlIgnore] // No serializar esta propiedad para el historial
        public decimal TotalCarrito
        {
            get => _totalManual ? _totalCarrito : PrecioVenta * Cantidad;
            set
            {
                _totalCarrito = value;
                _totalManual = true;
                OnPropertyChanged(nameof(TotalCarrito));
            }
        }

        [XmlIgnore] // No serializar esta propiedad para el historial
        public int Cantidad
        {
            get => _cantidad;
            set
            {
                if (_cantidad != value)
                {
                    _cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                    TotalCarrito = PrecioVenta * _cantidad; // Actualizar total al cambiar cantidad
                }
            }
        }


        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor sin parámetros requerido para serialización XML
        public ArticuloDTO()
        {
            // Inicializar propiedades que no aceptan null
            Descripcion = string.Empty;
            idCategoria = 0;
            CodigoBarras = string.Empty;
            Color = string.Empty;
            Tamanio = string.Empty;
            Foto = string.Empty;
        }
    }
}