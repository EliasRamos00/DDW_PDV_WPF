using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmVentanaPrincipal.xaml
    /// </summary>
    public partial class frmVentanaPrincipal : Window
    {
        public frmVentanaPrincipal()
        {
            InitializeComponent();
            MainFrame.Navigate(new frmVentas());
        }

        private void NavigateToInventarios(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new frmInventarios());
        }

        private void NavigateToVentas(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new frmVentas());
        }

        private void NavigateHistorial(object sender, RoutedEventArgs e)
        {
           // MainFrame.Navigate(new Page2());
        }
    }
}
