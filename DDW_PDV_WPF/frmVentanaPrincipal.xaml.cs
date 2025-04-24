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
        GoogleDriveHelper ds = new GoogleDriveHelper();

        public frmVentanaPrincipal(string Usuario, string Rol)
        {
            InitializeComponent();
            MainFrame.Navigate(new frmVentas(Usuario, ds));
            MainFrame.UpdateLayout();
            Permisos(Rol);
            this.Usuario = Usuario;
        }

        private void Permisos(string rol)
        {
            if (rol.Equals("admin"))
            {
                buttonInventario.Visibility = Visibility.Visible;
                buttonResumen.Visibility = Visibility.Visible; ;
                buttonHistorial.Visibility = Visibility.Visible;
                buttonCierreCajas.Visibility = Visibility.Visible;
            }
            else
            {
                buttonInventario.Visibility = Visibility.Collapsed;
                buttonResumen.Visibility = Visibility.Collapsed;
                buttonHistorial.Visibility = Visibility.Collapsed;
                buttonCierreCajas.Visibility = Visibility.Collapsed;
            } 
          
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
            MainFrame.Navigate(new frmInventarios(ds));
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
            MainFrame.Navigate(new frmVentas(Usuario, ds));
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
            // DESCOMENTAR UNA VEZ TERMINADAS LAS PRUEBAS EN DROPBOX
            CierrCaj paginaDestino = new CierrCaj(Usuario, Properties.Settings.Default.Caja);
            paginaDestino.ShowDialog();           

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

        private void ConfigClick(object sender, RoutedEventArgs e)
        {
            ConfigCajaSucursal configWindow = new ConfigCajaSucursal();
            configWindow.ShowDialog();
        }
    }
}
