using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class MProveedores
    {
        public int idProveedor { get; set; }
        public string Nombre { get; set; }
        public int? Estado { get; set; }
        public string Domicilio { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Foto { get; set; }
        public DateTime? FechaRegistro { get; set; }

    }
}
