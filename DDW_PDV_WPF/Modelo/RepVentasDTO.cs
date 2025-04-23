using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class RepVentasDTO
    {

        public DateTime FechaHora { get; set; }
        public string Descripcion { get; set; }
        public int GrupoVenta { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal Ganancia { get; set; }
        public int Cantidad { get; set; }



    }
}
