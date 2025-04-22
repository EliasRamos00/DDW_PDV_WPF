using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
   public class MReportesDTO
    {
        public decimal Ventas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal Ganancia { get; set; }
        public string ProductoMasVendido { get; set; }
        public int CantidadProductoMasVendido { get; set; }
        public string UsuarioMasVentas { get; set; }
    }
}
