using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Interaction logic for frmProveedores.xaml
    /// </summary>
    public partial class frmProveedores : Page
    {
        public frmProveedores()
        {
            InitializeComponent();
        }

        private void cbPDF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPDF.SelectedValue == null)
                return;

            if (cbPDF.SelectedValue.ToString().Contains("Seleccionar PDF"))
            {
                // Abrir selector de archivos
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Archivos PDF (*.pdf)|*.pdf",
                    Title = "Seleccionar PDF"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string rutaPdf = openFileDialog.FileName;

                    // Leer texto completo del PDF
                    string textoPdf = LeerTextoPdf(rutaPdf);

                    // Separar bloques de productos (PC1, PC2, etc.)
                    var matches = Regex.Matches(
                        textoPdf,
                        @"PC\d+.*?(?=PC\d+|$)",
                        RegexOptions.Singleline
                    );

                    var lineasProducto = matches
                        .Cast<Match>()
                        .Select(m => m.Value.Trim())
                        .ToList();

                    // Regex final corregida (código sin números al final)
                    var regex = new Regex(
                        @"(PC\d+)" +                                      // PC
                        @"([A-Z0-9]+-[A-Z]+(?:\/[A-Z]+)?)" +              // Código (ID)
                        @".*?" +                                          // Todo lo demás
                        @"(\d+\.\d{2})(?=\s*$)",                          // ✅ ÚLTIMO decimal = cantidad
                        RegexOptions.Singleline
                    );



                    // ===== AQUÍ EMPIEZA LO NUEVO =====

                    var productosPdf = new List<ProductoPdf>();

                    foreach (var linea in lineasProducto)
                    {
                        var match = regex.Match(linea);
                        if (!match.Success)
                            continue;

                        var producto = new ProductoPdf
                        {
                            Codigo = match.Groups[2].Value,
                            Cantidad = decimal.Parse(match.Groups[3].Value)
                        };

                        productosPdf.Add(producto);
                    }


                    // Debug: ver todos juntos
                    foreach (var p in productosPdf)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Codigo: {p.Codigo} | Cantidad: {p.Cantidad}"
                        );
                    }

                    // Aquí ya tienes TODOS los productos listos para:
                    // - comparar con BD
                    // - agrupar
                    // - mostrar en DataGrid
                }
            }
        }


        private string LeerTextoPdf(string rutaPdf)
        {
            StringBuilder textoCompleto = new StringBuilder();

            using (var pdf = UglyToad.PdfPig.PdfDocument.Open(rutaPdf))
            {
                foreach (var page in pdf.GetPages())
                {
                    textoCompleto.AppendLine($"--- PÁGINA {page.Number} ---");
                    textoCompleto.AppendLine(page.Text);
                }
            }

            return textoCompleto.ToString();
        }



    }
}


public class ProductoPdf
{
    public string Codigo { get; set; }
    public decimal Cantidad { get; set; }
}