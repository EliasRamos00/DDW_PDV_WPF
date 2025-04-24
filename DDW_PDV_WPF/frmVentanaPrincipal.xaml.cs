using DDW_PDV_WPF.Controlador;
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
        private void ResetNavigationButtons()
        {
            buttonInventario.IsEnabled = true;
            buttonCierreCajas.IsEnabled = true;
            buttonVentas.IsEnabled = true;
            buttonResumen.IsEnabled = true;
            buttonHistorial.IsEnabled = true;

        }
        private void NavigateToInventarios(object sender, RoutedEventArgs e)
        {
            ResetNavigationButtons();
            MainFrame.Navigate(new frmInventarios());
            buttonInventario.IsEnabled = false;
        }

        private void NavigateToCierreDeCajas(object sender, RoutedEventArgs e)
        {
            ResetNavigationButtons();
            MainFrame.Navigate(new frmCierreDeCajas());
            buttonCierreCajas.IsEnabled = false;
        }

        private void NavigateToVentas(object sender, RoutedEventArgs e)
        {
            ResetNavigationButtons();
            MainFrame.Navigate(new frmVentas(Usuario));
            buttonVentas.IsEnabled = false;
        }

        private void NavigateToReportes(object sender, RoutedEventArgs e)
        {
            ResetNavigationButtons();
            MainFrame.Navigate(new frmReportes());
            buttonResumen.IsEnabled = false;
        }

        private void NavigateHistorial(object sender, RoutedEventArgs e)
        {
            ResetNavigationButtons();
            MainFrame.Navigate(new frmHistorialModificaciones());
            buttonHistorial.IsEnabled = false;
        }

        private void NavigateCierre(object sender, RoutedEventArgs e)
        {
            ResetNavigationButtons();
            MainFrame.Navigate(new frmCierreDeCajas());
            buttonCierreCajas.IsEnabled = false;
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

        //CIERRE DE CAJA
        private async void btnCerrarCaja(object sender, RoutedEventArgs e)
        {
            //// DESCOMENTAR UNA VEZ TERMINADAS LAS PRUEBAS EN DROPBOX
            //    CierrCaj paginaDestino = new CierrCaj(Usuario, Properties.Settings.Default.Caja);
            //    paginaDestino.ShowDialog();

            // Crea una nueva instancia de OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.gif|Todos los archivos|*.*";

            // Abre el cuadro de diálogo para seleccionar el archivo
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) // true para WPF
            {
                string filePath = openFileDialog.FileName;

                try
                {
                    // Obtener el servicio autenticado de Google Drive
                    Controlador.GoogleDriveHelper.Initialize("neuralcat.json");

                    // ID de la carpeta compartida en tu Google Drive
                    string folderId = "1mljTxnPYGefWWFBbWe2V_lKxX7oeugdA";

                    // Sube el archivo y obtén el ID
                    string fileId = await Controlador.GoogleDriveHelper.UploadFileAsync(filePath, folderId);

                    // Obtén el link público (opcional)
                    string publicLink = await GoogleDriveHelper.GetPublicLinkAsync(fileId);

                    System.Windows.Forms.MessageBox.Show("Archivo subido con éxito. Enlace:\n" + fileId);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("Error al subir archivo: " + ex.Message);
                }
            }

        }

        private void clickBorrarCache(object sender, RoutedEventArgs e)
        {
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DriveCache");

            if (!Directory.Exists(cacheDirectory))
                return;

            try
            {
                Directory.Delete(cacheDirectory, true); // Elimina todo, incluyendo subdirectorios y archivos
                Directory.CreateDirectory(cacheDirectory); // La volvemos a crear vacía por si se sigue usando
                System.Windows.MessageBox.Show("¡Caché borrada correctamente!", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al borrar la caché: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
