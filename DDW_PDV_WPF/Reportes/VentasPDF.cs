using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Reportes
{
    class VentasPDF
    {
        public void CrearPDF(List<Modelo.RepVentasDTO> productos, string rutaSalida)
        {

            // Crear documento
            var doc = new Document();
            var seccion = doc.AddSection();

            // Agregar título
            var titulo = seccion.AddParagraph("Reporte de Productos");
            titulo.Format.Font.Size = 16;
            titulo.Format.Font.Bold = true;
            titulo.Format.SpaceAfter = "1cm";

            // Crear tabla
            var tabla = seccion.AddTable();
            tabla.Borders.Width = 0.75;

            // Definir columnas
            tabla.AddColumn("4cm");
            tabla.AddColumn("3cm");
            tabla.AddColumn("3cm");

            // Encabezados
            var filaEncabezado = tabla.AddRow();
            filaEncabezado.Shading.Color = Colors.LightGray;
            filaEncabezado.Cells[0].AddParagraph("Nombre");
            filaEncabezado.Cells[1].AddParagraph("Precio");
            filaEncabezado.Cells[2].AddParagraph("Cantidad");

            // Filas de datos
            foreach (var p in productos)
            {
                var fila = tabla.AddRow();
                fila.Cells[0].AddParagraph(p.Descripcion);
                fila.Cells[1].AddParagraph(p.PrecioVenta.ToString("C"));
                fila.Cells[2].AddParagraph(p.Ganancia.ToString());
            }

            // Renderizar y guardar como PDF
            var renderizador = new PdfDocumentRenderer(true)
            {
                Document = doc
            };
            renderizador.RenderDocument();
            renderizador.PdfDocument.Save(rutaSalida);

            // Abrir el PDF generado
            Process.Start(rutaSalida);
        }



    }
}
