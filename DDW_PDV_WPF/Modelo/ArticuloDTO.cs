using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class ArticuloDTO : INotifyPropertyChanged
    {
        private int _cantidad;
        private decimal _totalCarrito;

        public int idArticulo { get; set; }
        public string Foto { get; set; }
        public string Color { get; set; }
        public string Descripcion { get; set; }
        public string Tamanio { get; set; }
        public string CodigoBarras { get; set; }
        public int IdCategoria { get; set; }

        // INVENTARIO
        public int idInventario { get; set; }
        public int? Stock { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal? PrecioCompra { get; set; }


        public decimal Precio { get; set; }

        public decimal TotalCarrito
        {
            get => _totalCarrito;
            set
            {              
                    _totalCarrito = PrecioVenta * _cantidad;
                    OnPropertyChanged(nameof(TotalCarrito));                
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
