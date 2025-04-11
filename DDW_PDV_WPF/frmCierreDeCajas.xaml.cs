using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmCierreDeCajas.xaml
    /// </summary>
    public partial class frmCierreDeCajas : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ApiService _apiService;
        private ObservableCollection<CierreCajasDTO> _listaCierres;
        private CierreCajasDTO _cierreSeleccionado;
        private string _textoBusqueda;
        private ObservableCollection<CierreCajasDTO> _todosLosCierres;

        public ObservableCollection<CierreCajasDTO> ListaCierres
        {
            get => _listaCierres;
            set
            {
                _listaCierres = value;
                OnPropertyChanged();
            }
        }

        public CierreCajasDTO CierreSeleccionado
        {
            get => _cierreSeleccionado;
            set
            {
                _cierreSeleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HayCierreSeleccionado));
            }
        }

        public bool HayCierreSeleccionado => CierreSeleccionado != null;

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                FiltrarCierres();
            }
        }
        public frmCierreDeCajas()
        {
            InitializeComponent();
            DataContext = this;
            _apiService = new ApiService();
            CargarDatos();
        }

         private async void CargarDatos()
    {
        try
        {
            var resultado = await _apiService.GetAsync<List<CierreCajasDTO>>("/api/CCierresCajas");

            if (resultado != null)
            {
                _todosLosCierres = new ObservableCollection<CierreCajasDTO>(
                    resultado.OrderByDescending(c => c.Fecha + c.Hora));
                ListaCierres = new ObservableCollection<CierreCajasDTO>(_todosLosCierres);
                

            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar los cierres de caja: {ex.Message}", "Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
       
        private void FiltrarCierres()
    {
        if (_todosLosCierres == null) return;

        if (string.IsNullOrWhiteSpace(TextoBusqueda))
        {
            ListaCierres = new ObservableCollection<CierreCajasDTO>(_todosLosCierres);
        }
        else
        {
            var texto = TextoBusqueda.ToLower();
            var resultados = _todosLosCierres
                .Where(c =>
                    (c.Fecha?.ToLower().Contains(texto) ?? false) ||
                    (c.Hora?.ToLower().Contains(texto) ?? false) ||
                    (c.idUsuario.ToString().Contains(texto)) ||
                    (c.idCaja.ToString().Contains(texto)) ||
                    (c.TotalSistema.ToString("C").ToLower().Contains(texto)) ||
                    (c.TotalFisico.ToString("C").ToLower().Contains(texto)) ||
                    (c.Diferencia.ToString("C").ToLower().Contains(texto)))
                .ToList();

            ListaCierres = new ObservableCollection<CierreCajasDTO>(resultados);
        }

        CierreSeleccionado = ListaCierres.FirstOrDefault();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is CierreCajasDTO cierre)
        {
            CierreSeleccionado = cierre;
        }
    }

    private void btnClearSearch_Click(object sender, RoutedEventArgs e)
    {
        TextoBusqueda = string.Empty;
        txtBusqueda.Focus();
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    }
}

