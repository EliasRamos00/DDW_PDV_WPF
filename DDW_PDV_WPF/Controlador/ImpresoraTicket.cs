using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Xps;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using DDW_PDV_WPF.Modelo;
using System.Collections.ObjectModel;

namespace DDW_PDV_WPF.Controlador
{
    public class ImpresoraTicket
    {


        public static void ImprimeTicket(ObservableCollection<ArticuloDTO> productos, decimal totalCarro)
        {
           

            string RFC = "GUSY970729 868", Nombre = "YECENIA GURROLA SANCHEZ", Dirr = "PASTEUR 301 SUR", Cel = "618 230 9875", Ciudad = "DURANGO DGO.", CP = "34000";


            // Crear el FlowDocument
            FlowDocument flowDoc = new FlowDocument();

            // Configurar la fuente y el tamaño
            flowDoc.FontFamily = new FontFamily("Arial");
            flowDoc.FontSize = 10; // Ajusta el tamaño de la fuente según necesidad

            // Crear un párrafo para la imagen (por ejemplo, logo en la parte superior)
            Paragraph imageParagraph = new Paragraph();
            imageParagraph.TextAlignment = TextAlignment.Center; // Centrar la imagen
                                                               // Obtener la ruta del directorio raíz del proyecto
            string rutaRaiz = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName).FullName;

            // Ruta completa del archivo de imagen en la carpeta Resources
            string rutaImagen = System.IO.Path.Combine(rutaRaiz, "Resources", "omegasymbol.png");

            // Crear la URI a partir de la ruta combinada
            Uri imagenUri = new Uri(rutaImagen);


            Image logoImage = new Image
            {
                Source = new BitmapImage(imagenUri),
                Height = 150, // Ajusta la altura de la imagen según necesidad
                Width = 150,
                Margin = new Thickness(0),
                MaxHeight = 150,
                MaxWidth = 150

            };

            // Añadir la imagen al párrafo
            //imageParagraph.Inlines.Add(new Run(""));            

            imageParagraph.Inlines.Add(new InlineUIContainer(logoImage));
            flowDoc.Blocks.Add(imageParagraph); //AGREGAR EN CASO DE NECESITAR LA IMAGEN ******************

            // Crear un párrafo con contenido para el encabezado "TICKET"
            Paragraph headerParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center, // Centrado del encabezado
                Margin = new Thickness(0),
                FontSize = 14, // Tamaño de fuente más grande para el encabezado

            };

            // Añadir el texto de encabezado
            headerParagraph.Inlines.Add(new Run(" Flores Artificiales y Artesanias Omega \n"));
            flowDoc.Blocks.Add(headerParagraph);

            // Crear un párrafo con la fecha
            Paragraph dateParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center, // Alineación centrada
                Margin = new Thickness(0)
            };


            dateParagraph.Inlines.Add(new Run($"FECHA: {DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")}\n"));
            dateParagraph.Inlines.Add(new Run($"DIR: {Dirr}\n"));
            dateParagraph.Inlines.Add(new Run($"{Nombre}\n"));
            dateParagraph.Inlines.Add(new Run($"RFC: {RFC}\n"));
            dateParagraph.Inlines.Add(new Run($"TEL: {Cel}\n"));
            dateParagraph.Inlines.Add(new Run($"{Ciudad}\n"));
            dateParagraph.Inlines.Add(new Run($"CP: {CP}\n\n\n"));

            flowDoc.Blocks.Add(dateParagraph);


            // Crear un párrafo con la fecha
            Paragraph Encabezado = new Paragraph
            {
                TextAlignment = TextAlignment.Left, // Alineación centrada
                Margin = new Thickness(0)
            };

            Encabezado.Inlines.Add(new Run($"(Pzas).  Descrip.  \t\t Precio.\n"));

            flowDoc.Blocks.Add(Encabezado);

            // Crear la tabla
            Table table = new Table();
            table.Columns.Add(new TableColumn() { Width = new GridLength(100, GridUnitType.Pixel) }); // Columna para el nombre
            table.Columns.Add(new TableColumn() { Width = new GridLength(80, GridUnitType.Pixel) }); // Columna para el precio

            // Añadir una fila para cada producto
            foreach (ArticuloDTO producto in productos)
            {
                TableRow row = new TableRow();

                // Celda para el nombre del producto (alineado a la izquierda)
                TableCell nameCell = new TableCell(new Paragraph(new Run($"({producto.Cantidad}) " + producto.Descripcion)))
                {
                    TextAlignment = TextAlignment.Left,
                    Padding = new Thickness(0)
                };

                int totalPorProd = Convert.ToInt32(producto.Cantidad) * Convert.ToInt32(producto.PrecioVenta);

                // Celda para el precio (alineado a la derecha)
                TableCell priceCell = new TableCell(new Paragraph(new Run(producto.TotalCarrito.ToString("C2"))))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(0,0,20,0)
                };

                // Agregar celdas a la fila
                row.Cells.Add(nameCell);
                row.Cells.Add(priceCell);

                // Agregar fila a la tabla
                table.RowGroups.Add(new TableRowGroup() { Rows = { row } });
            }

            // Agregar la tabla al documento
            flowDoc.Blocks.Add(table);

            // Crear un párrafo con el total
            Paragraph totalParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Right, // Alineación a la izquierda para el total
                Margin = new Thickness(0,0,6,0)
            };
            totalParagraph.Inlines.Add(new Run("\n\n\n"));
            totalParagraph.Inlines.Add(new Run($"TOTAL: {totalCarro.ToString("C2")}\n")); // Esto puede ser calculado dinámicamente también
            flowDoc.Blocks.Add(totalParagraph);

            // Crear un párrafo con el mensaje de agradecimiento
            Paragraph thankYouParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center, // Centrado del mensaje de agradecimiento
                Margin = new Thickness(0)
            };

            thankYouParagraph.Inlines.Add(new Run("\n\n"));
            thankYouParagraph.Inlines.Add(new Run("Gracias por su compra\n"));
            thankYouParagraph.Inlines.Add(new Run("********************************\n"));
            thankYouParagraph.Inlines.Add(new Run("\n\n"));

            flowDoc.Blocks.Add(thankYouParagraph);

            // Configurar el FlowDocument con el tamaño de página (ancho 57.5mm = 220.472px aprox.)
            flowDoc.PageWidth = 180; // 57.5 mm en píxeles a 96 DPI
            flowDoc.PageHeight = 2000;    // Ajusta la altura según la longitud del ticket
            flowDoc.PagePadding = new Thickness(5);  // Márgenes pequeños

            // Llamar a la función para imprimir el FlowDocument
            PrintFlowDocument(flowDoc);

        }

        private static void PrintFlowDocument(FlowDocument flowDoc)
        {

            try
            {
                // Obtener la impresora predeterminada del sistema
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueue printQueue = printServer.DefaultPrintQueue;

                // Crear un escritor para la impresora
                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);

                // Imprimir el documento FlowDocument
                writer.Write(((IDocumentPaginatorSource)flowDoc).DocumentPaginator);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al imprimir: " + ex.Message);
            }

        }

    }
}
