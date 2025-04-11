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
        private void NavigateToCierreDeCajas(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new frmCierreDeCajas());
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
            MainFrame.Navigate(new frmHistorialModificaciones());
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
        private void btnCerrarCaja(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("¿Está seguro que desea cerrar la caja?",
                               "Confirmar cierre de caja",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {

                // Navegar a la nueva página si el usuario confirma
                CierrCaj paginaDestino = new CierrCaj(Usuario, Properties.Settings.Default.Caja);
                paginaDestino.Show();
            }
        }
    }
}
