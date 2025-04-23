using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDW_PDV_WPF.Reportes
{
    public class VentasPDF
    {
        public static void CrearPDF(List<Modelo.RepVentasDTO> productos, string rutaSalida, DateTime fechaini, DateTime fechafin, int Sucursal)
        {
            string RFC = "GUSY970729 868", Nombre = "YECENIA GURROLA SANCHEZ", Dirr = "PASTEUR 301 SUR", Cel = "618 230 9875", Ciudad = "DURANGO DGO.", CP = "34000";
            string rutaImagen = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "omegasymbol.png");

            if (productos == null)
                productos = new List<Modelo.RepVentasDTO>();

            var doc = new Document();
            var seccion = doc.AddSection();

            // Margen inferior para número de página
            seccion.PageSetup.BottomMargin = "2.5cm";

            // Agregar encabezado con logo e info
            var encabezado = seccion.Headers.Primary.AddParagraph();
            var tablaHeader = seccion.AddTable();
            tablaHeader.Borders.Width = 0;
            tablaHeader.AddColumn("5cm");
            tablaHeader.AddColumn("10cm");

            var rowHeader = tablaHeader.AddRow();

            // Logo
            if (System.IO.File.Exists(rutaImagen))
            {
                rowHeader.Cells[0].AddImage(rutaImagen).Width = "2.5cm";
            }


            // Título principal
            var titulo = rowHeader.Cells[1].AddParagraph("REPORTE DE VENTAS Y GANANCIAS");
            titulo.Format.Font.Size = 18;
            titulo.Format.Font.Bold = true;
            titulo.Format.Alignment = ParagraphAlignment.Center;


            seccion.AddParagraph("\n");
          
            // Fechas y sucursal
            var infoPeriodo = rowHeader.Cells[1].AddParagraph($"Periodo: {fechaini:dd - MMMM - yyyy} al {fechafin:dd - MMMM - yyyy}   |   Sucursal ID: {Sucursal}");
            infoPeriodo.Format.Font.Size = 10;
            //infoPeriodo.Format.SpaceAfter = "0.3cm";
            infoPeriodo.Format.Alignment = ParagraphAlignment.Left;

            // Crear tabla de productos
            var tabla = seccion.AddTable();
            tabla.Borders.Width = 0.75;
            tabla.Rows.LeftIndent = Unit.FromCentimeter(-2); // Centrado aproximado

            // Columnas
            tabla.AddColumn("2cm"); // Fecha 0
            tabla.AddColumn("5cm"); // Descripcion 1
            tabla.AddColumn("2cm"); // GrupoVenta 2
            tabla.AddColumn("2cm"); // PrecioVenta 3
            tabla.AddColumn("2cm"); // Ganancia 4
            tabla.AddColumn("1cm"); // Cantidad 5
            tabla.AddColumn("3cm"); // PrecioVentaTotal 6
            tabla.AddColumn("3cm"); // GananciaTotal 7


            tabla.Columns[2].Format.Alignment = ParagraphAlignment.Center;
            tabla.Columns[3].Format.Alignment = ParagraphAlignment.Right;
            tabla.Columns[4].Format.Alignment = ParagraphAlignment.Right;
            tabla.Columns[4].Format.Alignment = ParagraphAlignment.Right;
            tabla.Columns[5].Format.Alignment = ParagraphAlignment.Center;
            tabla.Columns[6].Format.Alignment = ParagraphAlignment.Right;
            tabla.Columns[7].Format.Alignment = ParagraphAlignment.Right;

            tabla.Columns[0].Format.Font.Size = 7.5;
            tabla.Columns[1].Format.Font.Size = 9;
           
            tabla.Columns[3].Format.Font.Size = 9;
            tabla.Columns[4].Format.Font.Size = 9;
            tabla.Columns[6].Format.Font.Size = 9;
            tabla.Columns[7].Format.Font.Size = 9;




            // Encabezado
            var filaEncabezado = tabla.AddRow();
            filaEncabezado.Shading.Color = Color.FromRgb(0x2E, 0x5E, 0x4E); // #2E5E4E
            filaEncabezado.HeadingFormat = true;
            filaEncabezado.Format.Font.Bold = true;
            filaEncabezado.Format.Font.Color = Colors.White;
            filaEncabezado.Format.Alignment = ParagraphAlignment.Center;

            filaEncabezado.Cells[0].AddParagraph("FechaHora");
            filaEncabezado.Cells[1].AddParagraph("Nombre");
            filaEncabezado.Cells[2].AddParagraph("Grupo Venta");
            filaEncabezado.Cells[3].AddParagraph("Precio Venta");
            filaEncabezado.Cells[4].AddParagraph("Ganancia");
            filaEncabezado.Cells[5].AddParagraph("Cant.");
            filaEncabezado.Cells[6].AddParagraph("Total");
            filaEncabezado.Cells[7].AddParagraph("Ganancia Total");


            // Datos

            string grupoAnterior = null;
            bool usarGris = false;

            foreach (var p in productos)
            {
                // Verifica si el grupo de venta ha cambiado
                if (p.GrupoVenta.ToString() != grupoAnterior)
                    usarGris = !usarGris;

                grupoAnterior = p.GrupoVenta.ToString();

                var fila = tabla.AddRow();

                // Aplica el color de fondo basado en la bandera
                fila.Shading.Color = usarGris ? Color.FromRgb(220, 220, 220) : Colors.White;

                fila.Cells[0].AddParagraph(p.FechaHora.ToString("dd-MMM HH:mm"));
                fila.Cells[1].AddParagraph(p.Descripcion);
                fila.Cells[2].AddParagraph(p.GrupoVenta.ToString());
                fila.Cells[3].AddParagraph(p.PrecioVenta.ToString("C"));
                fila.Cells[4].AddParagraph(p.Ganancia.ToString("C"));
                fila.Cells[5].AddParagraph(p.Cantidad.ToString());
                fila.Cells[6].AddParagraph((p.PrecioVenta * p.Cantidad).ToString("C"));
                fila.Cells[7].AddParagraph((p.Ganancia * p.Cantidad).ToString("C"));



            }


            // Pie de página con número de página
            var pie = seccion.Footers.Primary.AddParagraph();
            pie.AddText("Página ");
            pie.AddPageField();
            pie.Format.Alignment = ParagraphAlignment.Right;
            pie.Format.Font.Size = 9;



            // Calcular totales
            decimal totalVenta =0;
            decimal totalGanancia=0;

            foreach (var p in productos)
            {
                totalVenta = totalVenta + (p.PrecioVenta * p.Cantidad);
                totalGanancia = totalGanancia + (p.Ganancia * p.Cantidad);

            }

            // Fila de totales
            var filaTotales = tabla.AddRow();
            filaTotales.Shading.Color = Colors.LightGray;
            filaTotales.Format.Font.Bold = true;

            // Puedes dejar celdas vacías o poner títulos
            filaTotales.Cells[0].MergeRight = 2; // Combina las primeras 3 columnas
            filaTotales.Cells[0].AddParagraph("TOTALES:");
            filaTotales.Cells[0].Format.Alignment = ParagraphAlignment.Right;

            // Agrega los totales en las últimas columnas
            filaTotales.Cells[6].AddParagraph(totalVenta.ToString("C"));
            filaTotales.Cells[7].AddParagraph(totalGanancia.ToString("C"));




            // Renderizar PDF
            var renderizador = new PdfDocumentRenderer(true)
            {
                Document = doc
            };
            renderizador.RenderDocument();
            try
            {
                renderizador.PdfDocument.Save(rutaSalida);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al renderizar el PDF: {ex.Message}. Revisa si no tienes un PDF con el mismo nombre abierto o en uso.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Abrir el archivo generado
            Process.Start(new ProcessStartInfo(rutaSalida) { UseShellExecute = true });


        }
    }
}
