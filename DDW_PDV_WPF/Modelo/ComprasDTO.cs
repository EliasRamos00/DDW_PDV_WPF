using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Modelo
{
    public class MCompras
    {

        public int idCompra { get; set; }
        public DateTime? FechaCompra { get; set; }
        public decimal Total { get; set; }
        public int idProveedor { get; set; }

    }

    public class MComprasDTO
    {

        public int idCompra { get; set; }
        public DateTime? FechaCompra { get; set; }
        public decimal Total { get; set; }
        public int idProveedor { get; set; }
        public DateTime? fechaRegistro { get; set; }

        public List<MComprasDetalleDTO> Detalles { get; set; }
    }

    public class MComprasDetalleDTO
    {
        public int idComprasDetalle { get; set; }
        public int idArticulo { get; set; }
        public int idCompra { get; set; }
        public decimal Cantidad { get; set; }
        public decimal? PrecioUnitario { get; set; }
    }
}
