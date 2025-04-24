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
    /// Interaction logic for ConfigCajaSucursal.xaml
    /// </summary>
    public partial class ConfigCajaSucursal : Window
    {
        public ConfigCajaSucursal()
        {
            InitializeComponent();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            // Guardar valores en los ajustes de la aplicación
            Properties.Settings.Default.Caja = txtNumeroCaja.Text;
            Properties.Settings.Default.Sucursal = txtSucursal.Text;

            // Persistir los cambios
            Properties.Settings.Default.Save();

            // Cerrar la ventana y devolver resultado positivo
            this.DialogResult = true;
            this.Close();
        }
    }
}
