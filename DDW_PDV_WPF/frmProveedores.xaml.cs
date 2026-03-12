using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace DDW_PDV_WPF
{
    public partial class frmProveedores : Page
    {
        public ObservableCollection<ProductoPdf> ProductosPdf { get; set; }
        public ObservableCollection<ArticuloDTO> ArticulosEncontrados { get; set; }

        ApiService api = new ApiService();

        public frmProveedores()
        {
            InitializeComponent();

            ProductosPdf = new ObservableCollection<ProductoPdf>();
            ArticulosEncontrados = new ObservableCollection<ArticuloDTO>();

            DataContext = this;

            CargarPDFGuardados();
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;

            var parent = ((Control)sender).Parent as UIElement;
            parent.RaiseEvent(eventArg);
        }

        private string ObtenerCarpetaPDF()
        {
            string carpeta = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "PDFsGuardados"
            );

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            return carpeta;
        }

        private void CargarPDFGuardados()
        {
            string carpeta = ObtenerCarpetaPDF();

            var archivos = Directory.GetFiles(carpeta, "*.pdf");

            foreach (var archivo in archivos)
            {
                cbPDF.Items.Add(new ComboBoxItem
                {
                    Content = Path.GetFileName(archivo),
                    Tag = archivo
                });
            }
        }

        private async void cbPDF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPDF.SelectedItem == null)
                return;

            ComboBoxItem item = cbPDF.SelectedItem as ComboBoxItem;

            if (item == null)
                return;

            string rutaPdf = "";

            // Si es PDF guardado
            if (item.Tag != null)
            {
                rutaPdf = item.Tag.ToString();
            }
            else
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Archivos PDF (*.pdf)|*.pdf",
                    Title = "Seleccionar PDF"
                };

                if (openFileDialog.ShowDialog() != true)
                    return;

                rutaPdf = openFileDialog.FileName;

                string carpetaDestino = ObtenerCarpetaPDF();
                string nombreArchivo = Path.GetFileName(rutaPdf);
                string destino = Path.Combine(carpetaDestino, nombreArchivo);

                // Guardar copia
                if (!File.Exists(destino))
                {
                    File.Copy(rutaPdf, destino);

                    cbPDF.Items.Add(new ComboBoxItem
                    {
                        Content = nombreArchivo,
                        Tag = destino
                    });
                }

                item.Content = nombreArchivo;
            }

            try
            {
                string textoPdf = LeerTextoPdf(rutaPdf);

                var productos = ExtraerProductos(textoPdf);

                ProductosPdf.Clear();

                foreach (var producto in productos)
                {
                    ProductosPdf.Add(producto);
                }

                await BuscarEnApi();

                MessageBox.Show(
                    "Productos detectados: " + ProductosPdf.Count,
                    "Carga completada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al procesar PDF:\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void btnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            await BuscarEnApi();
        }

        private async Task BuscarEnApi()
        {
            try
            {
                ArticulosEncontrados.Clear();

                ComboBoxItem item = cbProveedor.SelectedItem as ComboBoxItem;

                if (item == null)
                {
                    MessageBox.Show("Selecciona proveedor");
                    return;
                }

                int idProveedor = int.Parse(item.Tag.ToString());

                foreach (var producto in ProductosPdf)
                {
                    try
                    {
                        string codigo = producto.Codigo
                            .Trim()
                            .Replace("\n", "")
                            .Replace("\r", "")
                            .Replace(" ", "")
                            .ToUpper();

                        var articulos = await api.GetAsync<List<ArticuloDTO>>(
                            $"api/CCodigosProveedores/{codigo}?idProveedor={idProveedor}"
                        );

                        if (articulos == null || articulos.Count == 0)
                        {
                            ArticulosEncontrados.Add(new ArticuloDTO
                            {
                                Cantidad = (int)producto.Cantidad,
                                Descripcion = "NO ENCONTRADO"
                            });

                            continue;
                        }

                        var articulo = articulos.First();

                        ArticulosEncontrados.Add(new ArticuloDTO
                        {
                            Cantidad = (int)producto.Cantidad,
                            Descripcion = articulo.Descripcion
                        });

                    }
                    catch
                    {
                        ArticulosEncontrados.Add(new ArticuloDTO
                        {
                            Cantidad = (int)producto.Cantidad,
                            Descripcion = "ERROR"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private List<ProductoPdf> ExtraerProductos(string textoPdf)
        {
            var productos = new List<ProductoPdf>();

            var bloques = Regex.Split(textoPdf, @"(?=PC\d+)");
            var contProd = 0;

            foreach (var bloque in bloques)
            {
                if (!bloque.StartsWith("PC"))
                    continue;

                var codigoMatch = Regex.Match(bloque, @"PC\d+([A-Z0-9\-\/]+?)(?=\s|\d+\.\d)");

                if (!codigoMatch.Success)
                    continue;

                string codigo = codigoMatch.Groups[1].Value;

                var numeros = Regex.Matches(bloque, @"\d+\.\d{2}");

                if (numeros.Count == 0)
                    continue;

                string cantidadTexto = "";

                if (numeros.Count != 5)
                {
                    cantidadTexto = numeros[numeros.Count - 7].Value;
                }
                else
                {
                    cantidadTexto = numeros[numeros.Count - 1].Value;
                }

                if (!decimal.TryParse(
                    cantidadTexto,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal cantidad))
                    continue;

                productos.Add(new ProductoPdf
                {
                    Codigo = codigo.TrimStart('0'),
                    Cantidad = cantidad
                });

                contProd++;
            }

            return productos;
        }

        private string LeerTextoPdf(string rutaPdf)
        {
            StringBuilder textoCompleto = new StringBuilder();

            using (UglyToad.PdfPig.PdfDocument pdf =
                   UglyToad.PdfPig.PdfDocument.Open(rutaPdf))
            {
                foreach (var page in pdf.GetPages())
                {
                    textoCompleto.AppendLine(page.Text);
                }
            }

            return textoCompleto.ToString();
        }
    }

    public class ProductoPdf
    {
        public string Codigo { get; set; }
        public decimal Cantidad { get; set; }
    }
}