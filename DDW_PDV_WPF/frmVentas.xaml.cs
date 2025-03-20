using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmVentas.xaml
    /// </summary>
    public partial class frmVentas : Page
    {
        private DispatcherTimer _timer;
        public frmVentas()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Actualizar cada segundo
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Actualizar la fecha y hora al iniciar
            UpdateDateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Actualizar la fecha y hora en cada tick del timer
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            // Obtener la fecha y hora actual
            DateTime now = DateTime.Now;

            // Formatear la fecha
            txtFecha.Text = now.ToString("dddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("es-MX"));

            // Formatear la hora con minutos y segundos
            txtHora.Text = now.ToString("HH:mm:ss", new CultureInfo("es-MX")); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el botón que se hizo clic
            Button clickedButton = sender as Button;

            // Restablecer el estilo de todos los botones
            foreach (var child in ((StackPanel)clickedButton.Parent).Children)
            {
                if (child is Button button)
                {
                    button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B8B8B"));
                    button.Background = new SolidColorBrush(Colors.Transparent);
                }
            }

            // Aplicar el estilo al botón seleccionado
            clickedButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            clickedButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
