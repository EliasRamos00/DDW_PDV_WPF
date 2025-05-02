using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    public partial class ConfigCajaSucursal : Window
    {
        private readonly ApiService _apiService = new ApiService();

        public ConfigCajaSucursal()
        {
            InitializeComponent();
        }

        private async void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtNumeroCaja.Text, out int numeroCaja) || !int.TryParse(txtSucursal.Text, out int idSucursal))
            {
                MessageBox.Show("Debes ingresar números válidos para la caja y la sucursal.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Obtener sucursales y cajas existentes desde la API
                var sucursales = await _apiService.GetAsync<List<MSucursalesDTO>>("/api/CSucursales");
                var cajas = await _apiService.GetAsync<List<MCajasDTO>>("/api/CCajas");

                // Verificar si la sucursal existe
                if (!sucursales.Any(s => s.idSucursal == idSucursal))
                {
                    MessageBox.Show($"La sucursal con ID {idSucursal} no existe.", "Sucursal no válida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Verificar si la caja ya existe
                var cajaExistente = cajas.FirstOrDefault(c => c.NumeroCaja == numeroCaja);

                if (cajaExistente != null && cajaExistente.idSucursal != idSucursal)
                {
                    MessageBox.Show($"La caja número {numeroCaja} ya está asignada a otra sucursal (Sucursal {cajaExistente.idSucursal}).", "Caja no válida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Guardar los valores si todo es válido
                Properties.Settings.Default.Caja = numeroCaja.ToString();
                Properties.Settings.Default.Sucursal = idSucursal.ToString();
                Properties.Settings.Default.Save();

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al validar la información: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

