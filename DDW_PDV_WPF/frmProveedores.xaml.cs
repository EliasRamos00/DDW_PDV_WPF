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

        public ObservableCollection<MProveedores> Proveedores { get; set; }

        public ObservableCollection<ArticuloDTO> ListaArticulos { get; set; }


        ApiService api = new ApiService();

        public frmProveedores()
        {
            InitializeComponent();

            ProductosPdf = new ObservableCollection<ProductoPdf>();
            ArticulosEncontrados = new ObservableCollection<ArticuloDTO>();
            Proveedores = new ObservableCollection<MProveedores>();
            ListaArticulos = new ObservableCollection<ArticuloDTO>();
            DataContext = this;
            CargarArticulosDesdeAPI();
            CargarPDFGuardados();
            CargarProveedoresDesdeAPI();

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
        private async void CargarProveedoresDesdeAPI()
        {
            try
            {
                var resultadoAPI = await api.GetAsync<List<MProveedores>>("/api/CProveedores");

                if (resultadoAPI != null)
                {
                    Proveedores.Clear();

                    foreach (var prov in resultadoAPI)
                    {
                        Proveedores.Add(new MProveedores
                        {
                            idProveedor = prov.idProveedor,
                            Nombre = prov.Nombre ?? "Sin Nombre",

                        });
                    }
                }
                cbProveedor.ItemsSource = Proveedores;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error al descargar los proveedores: " + ex.Message, "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CargarArticulosDesdeAPI()
        {
            try
            {
                var resultadoAPI = await api.GetAsync<List<ArticuloDTO>>("/api/CArticulos/productos/inventario");

                if (resultadoAPI != null)
                {
                    ListaArticulos.Clear(); // RETOMAR DESDE AQUI ELIAS DEL FUTURO

                    foreach (var articulo in resultadoAPI)
                    {
                        ListaArticulos.Add(new ArticuloDTO
                        {
                            idArticulo = articulo.idArticulo,
                            Descripcion = articulo.CodigoBarras +" - "+ articulo.Descripcion + " - " + articulo.Color ?? "Sin Nombre",
                            PrecioCompra = articulo.PrecioCompra ?? 0
                        });
                    }
                    cbArticuloManual.ItemsSource = ListaArticulos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al descargar los artículos: " + ex.Message,
                    "Error de Conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnAgregarArticulo_Click(object sender, RoutedEventArgs e)
        {
            int idArticulo = (cbArticuloManual.SelectedItem as ArticuloDTO)?.idArticulo ?? 0;
            int cantidad = Convert.ToInt16(txtCantidadManual.Text);
            decimal precio = Convert.ToDecimal(txtPrecioManual.Text);
            var articuloExistente = ArticulosEncontrados
                .FirstOrDefault(x => x.idArticulo == idArticulo);

            if (articuloExistente != null)
            {
                articuloExistente.Cantidad += cantidad;
                // Opcional: actualizar el último precio capturado
                articuloExistente.PrecioCompra = precio;
            }
            else
            {
                ArticulosEncontrados.Add(new ArticuloDTO
                {
                    idArticulo = idArticulo,
                    Cantidad = cantidad,
                    Descripcion = cbArticuloManual.Text,
                    PrecioCompra = Convert.ToDecimal(txtPrecioManual.Text)
                });
            }

            CalcularTotal();

        }


        private void btnEliminarArticulo_Click(object sender, RoutedEventArgs e)
        {

            if (sender is Button btn && btn.DataContext is ArticuloDTO articulo)
            {
                ArticulosEncontrados.Remove(articulo);
            }
            CalcularTotal();

        }

        private void CalcularTotal()
        {
            decimal? total = 0;

            foreach (var articulo in ArticulosEncontrados)
            {
                total += articulo.Cantidad * articulo.PrecioCompra;
            }

            txtBTotalCompra.Text = total?.ToString("F2");
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

        private async void CargarCompra_Click(object sender, RoutedEventArgs e)
        {           
            // UNA COMPRA NECESITA 
            // id Compra, Fecha Compra, Total, idProveedor, FechaRegistro, DetalleCompra (idCompraDetalle, idArticulo, idCompra, Cantidad, PrecioUnitario)
            if (ArticulosEncontrados.Count == 0)
            {
                MessageBox.Show("No hay artículos para cargar. Agrega artículos antes de cargar la compra.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            MComprasDTO compra = new MComprasDTO();
            var proveedor = (MProveedores)cbProveedor.SelectedItem;
            List<MComprasDetalleDTO> ListaCompra = new List<MComprasDetalleDTO>(); 

            compra.idCompra = 0;
            compra.FechaCompra = DateTime.Now;
            compra.Total = Convert.ToDecimal(txtBTotalCompra.Text);
            compra.idProveedor = proveedor.idProveedor;
            compra.fechaRegistro = DateTime.Now;

            // Se llena la lista de detalles.

            foreach (ArticuloDTO articulo in ArticulosEncontrados)
            {
                ListaCompra.Add(new MComprasDetalleDTO
                {
                    idComprasDetalle = 0,
                    idCompra = 0,
                    idArticulo = articulo.idArticulo,
                    Cantidad = articulo.Cantidad,
                    PrecioUnitario = articulo.PrecioCompra
                });
            }

            compra.Detalles = ListaCompra;

            // Con la compra construida, se envia a la API

            try {

                var resultado = await api.PostAsync("api/CCompras/", compra);
                MessageBox.Show("Compra cargada con exito!.", "Exito!", MessageBoxButton.OK, MessageBoxImage.Information);
                ArticulosEncontrados.Clear();
                txtBTotalCompra.Text = "0";
            }
            catch (Exception ex) { 
            
            MessageBox.Show(ex.Message);
            }

        }

        private void cbArticuloManual_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtPrecioManual.Text = (cbArticuloManual.SelectedItem as ArticuloDTO)?.PrecioCompra?.ToString("F2") ?? "0.00";
        }
    }

    public class ProductoPdf
    {
        public string Codigo { get; set; }
        public decimal Cantidad { get; set; }
    }
}