using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    using System;

    public class HistorialDTO
    {
        public int idHistorial { get; set; }
        public DateTime fechaHora { get; set; }
        public string idUsuario { get; set; }
        public string accion { get; set; }  
        public string clase { get; set; }   
        public string antes { get; set; }  
        public string despues { get; set; }
     
    }
}
