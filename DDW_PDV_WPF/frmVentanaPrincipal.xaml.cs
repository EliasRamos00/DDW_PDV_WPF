using System;
using System.Collections.Generic;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps;



namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmVentanaPrincipal.xaml
    /// </summary>
    public partial class frmVentanaPrincipal : Window
    {
        private string Usuario { get; set; }

        public frmVentanaPrincipal(string Usuario)
        {
            InitializeComponent();
            MainFrame.Navigate(new frmVentas(Usuario));
            MainFrame.UpdateLayout();
            this.Usuario = Usuario;
        }

        private void NavigateToInventarios(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new frmInventarios());
        }

        private void NavigateToVentas(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new frmVentas(Usuario));
        }
        private void NavigateToReportes(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new frmReportes());
        }

        private void NavigateHistorial(object sender, RoutedEventArgs e)
        {
           // MainFrame.Navigate(new Page2());
        }

        private void ImprimeTicket(object sender, RoutedEventArgs e)
        {
            // Lista de productos y precios
            var productos = new List<Tuple<string, decimal>>()
            {
              
                new Tuple<string, decimal>("Flor Cafe 2", 204.50m),
                new Tuple<string, decimal>("Flor Amarilla con Cafe 20304 SN.", 204.01m),
                new Tuple<string, decimal>("Flores Rosas 1", 10.76m)

            };

            string RFC="GUSY970729", Nombre="YECENIA GURROLA SANCHEZ", Dirr="PASTEUR 301 SUR", Cel="618 230 9875", Ciudad="DURANGO DGO.", CP="34000";


            // Crear el FlowDocument
            FlowDocument flowDoc = new FlowDocument();

            // Configurar la fuente y el tamaño
            flowDoc.FontFamily = new FontFamily("Arial");
            flowDoc.FontSize = 10; // Ajusta el tamaño de la fuente según necesidad

            // Crear un párrafo para la imagen (por ejemplo, logo en la parte superior)
            Paragraph imageParagraph = new Paragraph();
            imageParagraph.TextAlignment = TextAlignment.Left; // Centrar la imagen
                                                               // Obtener la ruta del directorio raíz del proyecto
            string rutaRaiz = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName).FullName;

            // Ruta completa del archivo de imagen en la carpeta Resources
            string rutaImagen = System.IO.Path.Combine(rutaRaiz, "Resources", "DDW.png");

            // Crear la URI a partir de la ruta combinada
            Uri imagenUri = new Uri(rutaImagen);


            Image logoImage = new Image
            {
                Source = new BitmapImage(imagenUri),
                Height = 200, // Ajusta la altura de la imagen según necesidad
                Margin = new Thickness(0),
                MaxHeight = 200,
                MaxWidth = 200

            };

            // Añadir la imagen al párrafo
            //imageParagraph.Inlines.Add(new Run(""));

            imageParagraph.Inlines.Add(new InlineUIContainer(logoImage));
            //flowDoc.Blocks.Add(imageParagraph); AGREGAR EN CASO DE NECESITAR LA IMAGEN ******************

            // Crear un párrafo con contenido para el encabezado "TICKET"
            Paragraph headerParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center, // Centrado del encabezado
                Margin = new Thickness(0),
                FontSize = 14, // Tamaño de fuente más grande para el encabezado

            };

            // Añadir el texto de encabezado
            headerParagraph.Inlines.Add(new Run(" Flores Artifiales y Merceria Omega \n"));
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

            Encabezado.Inlines.Add(new Run($"Pzas.  Descrip. \t\t Precio.\n"));

            flowDoc.Blocks.Add(Encabezado);

            // Crear la tabla
            Table table = new Table();
            table.Columns.Add(new TableColumn() { Width = new GridLength(100, GridUnitType.Pixel) }); // Columna para el nombre
            table.Columns.Add(new TableColumn() { Width = new GridLength(80, GridUnitType.Pixel) }); // Columna para el precio

            // Añadir una fila para cada producto
            foreach (var producto in productos)
            {
                TableRow row = new TableRow();

                // Celda para el nombre del producto (alineado a la izquierda)
                TableCell nameCell = new TableCell(new Paragraph(new Run("(1) " + producto.Item1)))
                {
                    TextAlignment = TextAlignment.Left,
                    Padding = new Thickness(0)
                };

                // Celda para el precio (alineado a la derecha)
                TableCell priceCell = new TableCell(new Paragraph(new Run(producto.Item2.ToString("C2"))))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(0)
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
                Margin = new Thickness(0)
            };
            totalParagraph.Inlines.Add(new Run("\n\n\n"));
            totalParagraph.Inlines.Add(new Run("TOTAL: $35.50\n")); // Esto puede ser calculado dinámicamente también
            flowDoc.Blocks.Add(totalParagraph);

            // Crear un párrafo con el mensaje de agradecimiento
            Paragraph thankYouParagraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center, // Centrado del mensaje de agradecimiento
                Margin = new Thickness(0)
            };
            thankYouParagraph.Inlines.Add(new Run("Gracias por su compra\n"));
            thankYouParagraph.Inlines.Add(new Run("********************************\n"));
            flowDoc.Blocks.Add(thankYouParagraph);

            // Configurar el FlowDocument con el tamaño de página (ancho 57.5mm = 220.472px aprox.)
            flowDoc.PageWidth = 180; // 57.5 mm en píxeles a 96 DPI
            flowDoc.PageHeight = 2000;    // Ajusta la altura según la longitud del ticket
            flowDoc.PagePadding = new Thickness(5);  // Márgenes pequeños

            // Llamar a la función para imprimir el FlowDocument
            PrintFlowDocument(flowDoc);

        }

        private void PrintFlowDocument(FlowDocument flowDoc)
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

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {

            var result = System.Windows.MessageBox.Show("¿Está seguro que desea cerrar sesión?",
                               "Confirmar cierre de sesión",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                this.IsEnabled = false;

                try
                {
                    frmLogin loginWindow = new frmLogin();
                    loginWindow.Show();
                    this.Close();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al cerrar sesión: {ex.Message}",
                                  "Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                    this.IsEnabled = true;
                }
            }
        }

       

    }
}
