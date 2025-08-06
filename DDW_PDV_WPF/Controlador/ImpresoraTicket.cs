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
using System.Globalization;

namespace DDW_PDV_WPF.Controlador
{
    public class ImpresoraTicket
    {
        public static void ImprimeTicket(ObservableCollection<ArticuloDTO> productos, decimal totalCarro, decimal subTotalCarro)
        {
            try
            {
                var culturaMexicana = new CultureInfo("es-MX");

                string RFC = "GUSY970729 868", Nombre = "YECENIA GURROLA SANCHEZ",
                       Dirr = "PASTEUR 301 SUR", Cel = "618 230 9875", Ciudad = "DURANGO DGO.", CP = "34000";

                FlowDocument flowDoc = new FlowDocument
                {
                    FontFamily = new FontFamily("Arial"),
                    FontSize = 10,
                    PageWidth = 180,
                    PageHeight = 2000,
                    PagePadding = new Thickness(5)
                };

                // ===== Logo (con manejo de errores) =====
                try
                {
                    string rutaRaiz = Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName).FullName;
                    string rutaImagen = Path.Combine(rutaRaiz, "Resources", "omegasymbol.png");

                    if (File.Exists(rutaImagen))
                    {
                        Uri imagenUri = new Uri(rutaImagen);
                        Image logoImage = new Image
                        {
                            Source = new BitmapImage(imagenUri),
                            Height = 100,
                            Width = 100,
                            Margin = new Thickness(0),
                            MaxHeight = 100,
                            MaxWidth = 100
                        };

                        Paragraph imageParagraph = new Paragraph
                        {
                            TextAlignment = TextAlignment.Center
                        };
                        imageParagraph.Inlines.Add(new InlineUIContainer(logoImage));
                        flowDoc.Blocks.Add(imageParagraph);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar el logo del ticket:\n" + ex.Message, "Error de imagen", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // ===== Encabezado =====
                flowDoc.Blocks.Add(new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14,
                    Margin = new Thickness(0),
                    Inlines = { new Run(" Flores Artificiales y Artesanías Omega \n") }
                });

                flowDoc.Blocks.Add(new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0),
                    Inlines =
                    {
                        new Run($"FECHA: {DateTime.Now:dd/MM/yyyy hh:mm tt}\n"),
                        new Run($"DIR: {Dirr}\n"),
                        new Run($"{Nombre}\n"),
                        new Run($"RFC: {RFC}\n"),
                        new Run($"TEL: {Cel}\n"),
                        new Run($"{Ciudad}\n"),
                        new Run($"CP: {CP}\n\n\n")
                    }
                });

                flowDoc.Blocks.Add(new Paragraph
                {
                    TextAlignment = TextAlignment.Left,
                    Margin = new Thickness(0),
                    Inlines = { new Run($"(Pzas).  Descrip.  \t\t Precio.\n") }
                });

                // ===== Tabla de productos =====
                Table table = new Table();
                table.Columns.Add(new TableColumn { Width = new GridLength(100, GridUnitType.Pixel) });
                table.Columns.Add(new TableColumn { Width = new GridLength(80, GridUnitType.Pixel) });

                TableRowGroup group = new TableRowGroup();

                foreach (ArticuloDTO producto in productos)
                {
                    TableRow row = new TableRow();

                    TableCell nameCell = new TableCell(new Paragraph(new Run($"({producto.Cantidad}) {producto.Descripcion}")))
                    {
                        TextAlignment = TextAlignment.Left,
                        Padding = new Thickness(0)
                    };

                    TableCell priceCell = new TableCell(new Paragraph(new Run(producto.TotalCarrito.ToString("C2", culturaMexicana))))
                    {
                        TextAlignment = TextAlignment.Right,
                        Padding = new Thickness(0, 0, 20, 0)
                    };

                    row.Cells.Add(nameCell);
                    row.Cells.Add(priceCell);
                    group.Rows.Add(row);
                }

                table.RowGroups.Add(group);
                flowDoc.Blocks.Add(table);

                // ===== Total =====
                flowDoc.Blocks.Add(new Paragraph
                {
                    TextAlignment = TextAlignment.Right,
                    Margin = new Thickness(0, 0, 6, 0),
                    Inlines =
                    {
                        new Run("\n\n\n"),
                        new Run($"subtotal: {subTotalCarro.ToString("C2", culturaMexicana)}\n"),
                        new Run($"TOTAL: {totalCarro.ToString("C2", culturaMexicana)}\n")

                    }
                });

                // ===== Mensaje final =====
                flowDoc.Blocks.Add(new Paragraph
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0),
                    Inlines =
                    {
                        new Run("\n\n"),
                        new Run("Gracias por su compra\n"),
                        new Run("********************************\n"),
                        new Run("\n\n")
                    }
                });

                // ===== Imprimir =====
                PrintFlowDocument(flowDoc);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar ticket:\n" + ex.Message, "Error al imprimir", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void PrintFlowDocument(FlowDocument flowDoc)
        {
            try
            {
                LocalPrintServer printServer = new LocalPrintServer();
                PrintQueue printQueue = printServer.DefaultPrintQueue;

                XpsDocumentWriter writer = PrintQueue.CreateXpsDocumentWriter(printQueue);
                writer.Write(((IDocumentPaginatorSource)flowDoc).DocumentPaginator);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al enviar a impresión:\n" + ex.Message, "Error de impresión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
