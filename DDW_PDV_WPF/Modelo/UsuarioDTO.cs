using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class UsuarioDTO
    {
        public int idUsuario { get; set; }
        public string Foto { get; set; }
        public string Usuario { get; set; }

        public string Contra { get; set; }
        public DateTime FechaRegistro { get; set; }

    }
}
