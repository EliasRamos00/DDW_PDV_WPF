using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class CierreCajasDTO
    {
        public int idCierreCaja { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        // public string Fecha { get; set; }
        // public string Hora { get; set; }
        public int idUsuario { get; set; }
        public decimal TotalSistema { get; set; }
        public decimal TotalFisico { get; set; }
        public int idCaja { get; set; }


        public decimal Diferencia => TotalFisico - TotalSistema;
    }
}
