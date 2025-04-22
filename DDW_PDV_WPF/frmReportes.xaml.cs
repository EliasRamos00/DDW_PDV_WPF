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
              
                MessageBox.Show($"Error al cargar sucursales: {ex.Message}", "Error",
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
                MessageBox.Show($"Error: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void btnGenerarReporte(object sender, RoutedEventArgs e)
        {
            // Aquí puedes agregar la lógica para generar el reporte
            var usuarios = await _apiService.GetAsync<List<UsuarioDTO>>("/api/CUsuarios/");

        }
    }

    public class SucursalDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
