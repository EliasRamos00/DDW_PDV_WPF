﻿using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Lógica de interacción para CierrCaj.xaml
    /// </summary>
    public partial class CierrCaj : Window, INotifyPropertyChanged
    {
        private string _caja;
        private string _usuario;
        private string _sucursal;
        private DateTime _fechaHora;
        private decimal _totalFisico;
        private readonly ApiService _apiService;
        private decimal _totalSistema;
        private string _nombreUsuario;
        private decimal _diferencia;


        public decimal Diferencia
        {
            get => _diferencia;
            set
            {
                if (_diferencia != value)
                {
                    _diferencia = value;
                    OnPropertyChanged(nameof(Diferencia));
                }
            }
        }
        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                if (_nombreUsuario != value)
                {
                    _nombreUsuario = value;
                    OnPropertyChanged(nameof(NombreUsuario));
                }
            }
        }

        public decimal TotalSistema
        {
            get => _totalSistema;
            set
            {
                if (_totalSistema != value)
                {
                    _totalSistema = value;
                    OnPropertyChanged(nameof(TotalSistema));
                }
            }
        }

        public string Usuario { 
            get => _usuario;


            set 
            {
                if (_usuario != value)
                {
                    _usuario = value;
                    OnPropertyChanged(nameof(Usuario));
                }
            } 
        }

        public decimal TotalFisico
        {
            get => _totalFisico;
            set
            {
                if (_totalFisico != value)
                {
                    _totalFisico = value;
                    OnPropertyChanged(nameof(TotalFisico));
                    CalcularDiferencia(); // Llama a la función para calcular la diferencia
                }
            }
        }

        public DateTime FechaHora
        {
            get => DateTime.Now;          
        }


        public string Caja
        {
            get => _caja;

            set
            {
                if (_caja != value)
                {
                    _caja = value;
                    OnPropertyChanged(nameof(Caja));
                }
            }
        }

        public string Sucursal
        {
            get => _sucursal;
            set
            {
                if (_sucursal != value)
                {
                    _sucursal = value;
                    OnPropertyChanged(nameof(Sucursal));
                }
            }
        }

        public CierrCaj(string Usuario, string Caja )
        {
            InitializeComponent();
            DataContext = this;
            _usuario = Usuario;
            _caja = Caja;
            _sucursal = Properties.Settings.Default.Sucursal;
            _apiService = new ApiService();

            // Se calcula el total del sistema - Se llama la API para obtener el total del sistema
            setTotalSistema();
            //OnPropertyChanged(nameof(FechaHora));

        }

        public void CalcularDiferencia()
        {
            
                Diferencia = TotalFisico - TotalSistema;
                OnPropertyChanged(nameof(Diferencia));
                    
        }

        private async void setTotalSistema()
        {
            // se envia la caja y la sucursal
            string url = $"api/CCierresCajas/totalsistema/{Properties.Settings.Default.Caja}/{Properties.Settings.Default.Sucursal}/{DateTime.Now.ToString("yyyy-MM-dd")}";
            var aux = await _apiService.GetAsync<GenericMessageDTO>(url);

            // Esto esta mal y hay que cambiarse desde la API
            if (aux != null)
            {
                TotalSistema = decimal.Parse(aux.data);
                OnPropertyChanged(nameof(TotalSistema));

            }
            else
            {
                MessageBox.Show("Error al obtener el total del sistema.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CancelarCierre(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void  btnHacerCierre(object sender, RoutedEventArgs e)
        {
            if (TotalFisico == 0)
            {
                if (MessageBoxResult.No == MessageBox.Show("Total contado fisico en 0 - ¿Deseas continuar?", "Aviso", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    return;
                }
            }

            // Construimos el objeto para la API
            CierreCajasDTO cierreCajasDTO = new CierreCajasDTO
            {
                idCaja = int.Parse(Caja),
                Usuario = "",
                idUsuario = int.Parse(Usuario),
                TotalFisico = TotalFisico,
                TotalSistema = TotalSistema,
                Fecha = DateTime.Now.Date,
                Hora = DateTime.Now.TimeOfDay
            };

            // Llamamos a la API para hacer el cierre de caja
            try
            {
                var resultado = await _apiService.PostAsync("api/CCierresCajas", cierreCajasDTO);

                if (resultado)
                {
                    MessageBox.Show("Cierre de caja registrado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);                 
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error al registrar el cierre de caja.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en la conexión con la API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Permitir solo un punto decimal
            if (e.Text == ".")
            {
                // Si ya hay un punto, cancelar la entrada
                if (textBox.Text.Contains("."))
                {
                    e.Handled = true;
                    return;
                }

                // Si el punto es el primer carácter, agregar un "0" antes
                if (string.IsNullOrEmpty(textBox.Text) || textBox.CaretIndex == 0)
                {
                    textBox.Text = "0" + e.Text;
                    textBox.CaretIndex = 2; // Mover el cursor después del "0."
                    e.Handled = true;
                    return;
                }
            }

            // Validar que el texto resultante sea un número decimal válido
            if (!decimal.TryParse(newText, out _))
            {
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            // Si el texto termina en punto, agregar un "0" al final
            if (textBox.Text.EndsWith("."))
            {
                textBox.Text += "0";
            }

            // Si el texto está vacío, asignar "0"
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = "0";
            }

            // Actualizar el binding
            TotalFisico = decimal.Parse(textBox.Text);
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
