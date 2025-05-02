using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using DDW_PDV_WPF.Reportes;
using System.Windows.Forms;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Threading;

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmReportes.xaml
    /// </summary>
    public partial class frmReportes : Page, INotifyPropertyChanged
    {
        private DateTime _fechaInicio = DateTime.Now;
        private DateTime _fechaFin = DateTime.Now;
        private int _idSucursal = 1;
        private ObservableCollection<SucursalDTO> _sucursales;
        private MReportesDTO _reporte;
        private readonly ApiService _apiService = new ApiService();
        public event PropertyChangedEventHandler PropertyChanged;


        // Propiedades
        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set
            {
                if (_fechaInicio != value)
                {
                    _fechaInicio = value;
                    OnPropertyChanged();
                    CargarDatos(); // Opcional: auto-cargar al cambiar fecha
                }
            }
        }

        public DateTime FechaFin
        {
            get => _fechaFin;
            set
            {
                if (_fechaFin != value)
                {
                    _fechaFin = value;
                    OnPropertyChanged();
                    CargarDatos(); // Opcional: auto-cargar al cambiar fecha
                }
            }
        }

        public int IdSucursal
        {
            get => _idSucursal;
            set
            {
                if (_idSucursal != value)
                {
                    _idSucursal = value;
                    OnPropertyChanged();
                    CargarDatos(); // Opcional: auto-cargar al cambiar sucursal
                }
            }
        }

        public ObservableCollection<SucursalDTO> Sucursales
        {
            get => _sucursales;
            set
            {
                _sucursales = value;
                OnPropertyChanged();
            }
        }

        public MReportesDTO Reporte
        {
            get => _reporte;
            set
            {
                _reporte = value;
                OnPropertyChanged();
            }
        }

        public frmReportes()
        {
           InitializeComponent();
            DataContext=this;
            CargarSucursales();
            CargarDatos();

            CultureInfo ci = new CultureInfo("es-ES");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }

        private async void CargarSucursales()
        {
            try
            {
                var sucursales = await _apiService.GetAsync<List<MSucursalesDTO>>("/api/CSucursales");

                if (sucursales != null && sucursales.Any())
                {
                    Sucursales = new ObservableCollection<SucursalDTO>(
                        sucursales.Select(s => new SucursalDTO
                        {
                            Id = s.idSucursal,
                            Nombre = s.nombre
                        }));

                    IdSucursal = Sucursales.FirstOrDefault()?.Id ?? 1;
                }
                else
                {
     
                    Sucursales = new ObservableCollection<SucursalDTO>
            {
                new SucursalDTO { Id = 1, Nombre = "Sucursal Central" }
            };
                    IdSucursal = 1;
                }
            }
            catch (Exception ex)
            {

                System.Windows.MessageBox.Show($"Error al cargar sucursales: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);

                Sucursales = new ObservableCollection<SucursalDTO>
        {
            new SucursalDTO { Id = 1, Nombre = "Sucursal Central (Error)" }
        };
                IdSucursal = 1;
            }
        }
        private async void CargarDatos()
        {

            try
            {

                string url = $"/api/CReportes/{FechaInicio:yyyy-MM-dd},{FechaFin:yyyy-MM-dd},{IdSucursal}";
                var resultado = await _apiService.GetAsync<MReportesDTO>(url);
                Reporte = resultado ?? throw new Exception("No se recibieron datos");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var datePicker = sender as DatePicker;
            if (datePicker != null)
            {
                // Registrar todos los eventos necesarios
                datePicker.Loaded -= DatePicker_Loaded;
                datePicker.CalendarOpened += DatePicker_CalendarOpened;
                datePicker.CalendarClosed += DatePicker_CalendarClosed;
                datePicker.LostFocus += DatePicker_LostFocus;
                datePicker.SelectedDateChanged += DatePicker_SelectedDateChanged;

                ApplySpanishFormat(datePicker);
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySpanishFormat(sender as DatePicker);
        }

        private void DatePicker_CalendarOpened(object sender, RoutedEventArgs e)
        {
            // No necesitamos hacer nada aquí, pero el evento debe estar registrado
        }

        private void DatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            ApplySpanishFormat(sender as DatePicker);
        }

        private void DatePicker_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplySpanishFormat(sender as DatePicker);
        }

        private void ApplySpanishFormat(DatePicker datePicker)
        {
            if (datePicker?.SelectedDate != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var textBox = datePicker.Template.FindName("PART_TextBox", datePicker) as DatePickerTextBox;
                    if (textBox != null)
                    {
                        // Formato con mes en minúsculas
                        textBox.Text = datePicker.SelectedDate.Value.ToString("d-MMMM-yyyy", new CultureInfo("es-ES"));

                        // Si prefieres el mes con mayúscula inicial ("Abril"):
                        // textBox.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                        //     datePicker.SelectedDate.Value.ToString("d-MMMM-yyyy", new CultureInfo("es-ES")));
                    }
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void btnGenerarReporte(object sender, RoutedEventArgs e)
        {
            // Aquí puedes agregar la lógica para generar el reporte
            DateTime fechaIni, fechaFin;
            fechaIni = FechaInicio;
            fechaFin = FechaFin;
            int sucursal = IdSucursal;

            // Formatea las fechas al formato yyyy-MM-dd para evitar problemas de interpretación
            //string url = $"/reporte/r?fechaInicio={fechaIni:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&idSucursal={sucursal}";
            string url = $"/api/CVentas/{fechaIni:yyyy-MM-dd 00:00:00},{fechaFin:yyyy-MM-dd 23:59:59},{sucursal}";

            var reporte = await _apiService.GetAsync<List<RepVentasDTO>>(url);
            string path;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Guardar reporte";
                saveFileDialog.Filter = "Archivo PDF (*.pdf)|*.pdf|Archivo Excel (*.xlsx)|*.xlsx|Todos los archivos (*.*)|*.*";
                saveFileDialog.DefaultExt = "pdf"; // Puedes cambiarlo si el reporte no es PDF
                saveFileDialog.FileName = "ReporteVentas"; // Nombre sugerido

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Retorna la ruta seleccionada por el usuario
                    path = saveFileDialog.FileName;
                }
                else
                {
                    return; // Si canceló
                }
            }

            VentasPDF.CrearPDF(reporte,path, fechaIni,FechaFin,sucursal );

        }
    }


    public class SucursalDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
