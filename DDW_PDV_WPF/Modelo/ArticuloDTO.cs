using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class ArticuloDTO
    {
        public int idArticulo { get; set; }
        public string Foto { get; set; }
        public string Color { get; set; }
        public string Descripcion { get; set; }
        public string Tamanio { get; set; }
        public string CodigoBarras { get; set; }
        public int IdCategoria { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal PrecioCompra { get; set; }
    }
}
