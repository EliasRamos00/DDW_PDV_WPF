using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
using System.Xml.Serialization;
using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;

namespace DDW_PDV_WPF
{
    /// <summary>
    /// Lógica de interacción para frmHistorialModificaciones.xaml
    /// </summary>
    
    
    public partial class frmHistorialModificaciones : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ApiService _apiService;
        private ObservableCollection<HistorialDTO> _listaHistorial;
        private HistorialDTO _historialSeleccionado;
        private string _textoBusqueda;
        private ObservableCollection<HistorialDTO> _todosLosHistoriales;

        public ObservableCollection<HistorialDTO> ListaHistorial
        {
            get => _listaHistorial;
            set
            {
                _listaHistorial = value;
                OnPropertyChanged();
            }
        }

        public   HistorialDTO HistorialSeleccionado
        {
            get => _historialSeleccionado;
            set
            {
                _historialSeleccionado = value;
                OnPropertyChanged();

                if (value != null)
                {
                    // Decodificar los XML cuando se selecciona un item
                    DecodificarDatosHistorial(value);
                }
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged();
                FiltrarHistorial();
            }
        }
        public frmHistorialModificaciones()
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
                var resultado = await _apiService.GetAsync<List<HistorialDTO>>("/api/CHistoriales");

                if (resultado != null)
                {
                    _todosLosHistoriales = new ObservableCollection<HistorialDTO>(resultado);
                    ListaHistorial = new ObservableCollection<HistorialDTO>(resultado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el historial: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FiltrarHistorial()
        {
            if (_todosLosHistoriales == null) return;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                ListaHistorial = new ObservableCollection<HistorialDTO>(_todosLosHistoriales);
            }
            else
            {
                var texto = TextoBusqueda.ToLower();
                var resultados = _todosLosHistoriales
                    .Where(h =>
                    {
                        // Filtros básicos (originales)
                        var coincideBasico =
                            (h.Usuario != null && h.Usuario.ToLower().Contains(texto)) ||
                            (h.accion != null && h.accion.ToLower().Contains(texto)) ||
                            (h.fechaHora.ToString().Contains(texto)) ||
                            (h.clase != null && h.clase.ToLower().Contains(texto));

                        if (coincideBasico) return true;

                        // Filtro por datos "antes"
                        if (!string.IsNullOrEmpty(h.antes))
                        {
                            try
                            {
                                var articuloAntes = DeserializarArticuloParcial(h.antes);
                                if (articuloAntes != null &&
                                    articuloAntes.Descripcion != null &&
                                    articuloAntes.Descripcion.ToLower().Contains(texto))
                                {
                                    return true;
                                }
                            }
                            catch { /* Ignorar errores de deserialización */ }
                        }

                        // Filtro por datos "después"
                        if (!string.IsNullOrEmpty(h.despues))
                        {
                            try
                            {
                                var articuloDespues = DeserializarArticuloParcial(h.despues);
                                if (articuloDespues != null &&
                                    articuloDespues.Descripcion != null &&
                                    articuloDespues.Descripcion.ToLower().Contains(texto))
                                {
                                    return true;
                                }
                            }
                            catch { /* Ignorar errores de deserialización */ }
                        }

                        return false;
                    })
                    .ToList();

                ListaHistorial = new ObservableCollection<HistorialDTO>(resultados);
            }
        }

        // Método optimizado para deserializar solo lo necesario
        private ArticuloDTO DeserializarArticuloParcial(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return null;

            try
            {
                using (var reader = new StringReader(xml))
                {
                    var serializer = new XmlSerializer(typeof(ArticuloDTO));
                    return (ArticuloDTO)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return null;
            }
        }

        private void DecodificarDatosHistorial(HistorialDTO historial)
        {
            try
            {
                // Mostrar información básica
                txtFechaHora.Text = historial.fechaHora.ToString();
                txtUsuario.Text = historial.Usuario;
                txtTipoModificacion.Text = historial.accion;

                // Decodificar datos "antes"
                ArticuloDTO articuloAntes = null;
                if (!string.IsNullOrEmpty(historial.antes))
                {
                    articuloAntes = DeserializarDeXml<ArticuloDTO>(historial.antes);
                    if (articuloAntes != null)
                    {
                        // Cargar imagen ANTES
                        imgAntes.Source = CargarImagen(articuloAntes.Foto);
                        txtAntes.Text = FormatearDatosArticulo(articuloAntes);
                    }
                }

                // Decodificar datos "después"
                ArticuloDTO articuloDespues = null;
                if (!string.IsNullOrEmpty(historial.despues))
                {
                    articuloDespues = DeserializarDeXml<ArticuloDTO>(historial.despues);
                    if (articuloDespues != null)
                    {
                        // Cargar imagen DESPUÉS
                        imgDespues.Source = CargarImagen(articuloDespues.Foto);
                        txtDespues.Text = FormatearDatosArticulo(articuloDespues);
                    }
                }

                // Mostrar comparación de cambios
                txtMotivo.Text = CompararCambios(articuloAntes, articuloDespues);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al decodificar historial: {ex.Message}");
                txtMotivo.Text = "Error al mostrar los cambios";
            }
        }

        private BitmapImage CargarImagen(string rutaImagen)
        {
            if (string.IsNullOrEmpty(rutaImagen)) return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(rutaImagen, UriKind.RelativeOrAbsolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
            catch
            {
                return null;
            }
        }


        private string FormatearDatosArticulo(ArticuloDTO articulo)
        {
            if (articulo == null) return "N/A";
            return $@"ID: {articulo.idArticulo}
            Descripción: {articulo.Descripcion}
            Categoría: {articulo.idCategoria}
            Color: {articulo.Color}
            Tamaño: {articulo.Tamanio}
            Stock: {articulo.Stock}
            Mínimo: {articulo.Min}
            Máximo: {articulo.Max}
            Precio Venta: {articulo.PrecioVenta:C}
            Precio Compra: {articulo.PrecioCompra:C}
            Código: {articulo.CodigoBarras}";
        }

        private string CompararCambios(ArticuloDTO antes, ArticuloDTO despues)
        {
            if (antes == null && despues == null) return "No hay datos para comparar";

            if (antes == null) return "CREACIÓN: \n" + FormatearDatosArticulo(despues);
            if (despues == null) return "ELIMINACIÓN: \n" + FormatearDatosArticulo(antes);

            var cambios = new StringBuilder("CAMBIOS REALIZADOS:\n");

            if (antes.Descripcion != despues.Descripcion)
                cambios.AppendLine($"• Descripción: {antes.Descripcion} → {despues.Descripcion}");

            if (antes.idCategoria != despues.idCategoria)
                cambios.AppendLine($"• Categoría: {antes.idCategoria} → {despues.idCategoria}");

            if (antes.Color != despues.Color)
                cambios.AppendLine($"• Color: {antes.Color} → {despues.Color}");

            if (antes.Tamanio != despues.Tamanio)
                cambios.AppendLine($"• Tamaño: {antes.Tamanio} → {despues.Tamanio}");

            if (antes.Stock != despues.Stock)
                cambios.AppendLine($"• Stock: {antes.Stock} → {despues.Stock}");

            if (antes.Min != despues.Min)
                cambios.AppendLine($"• Mínimo: {antes.Min} → {despues.Min}");

            if (antes.Max != despues.Max)
                cambios.AppendLine($"• Máximo: {antes.Max} → {despues.Max}");

            if (antes.PrecioVenta != despues.PrecioVenta)
                cambios.AppendLine($"• Precio Venta: {antes.PrecioVenta:C} → {despues.PrecioVenta:C}");

            if (antes.PrecioCompra != despues.PrecioCompra)
                cambios.AppendLine($"• Precio Compra: {antes.PrecioCompra:C} → {despues.PrecioCompra:C}");

            if (antes.CodigoBarras != despues.CodigoBarras)
                cambios.AppendLine($"• Código: {antes.CodigoBarras} → {despues.CodigoBarras}");

            // Si no hay cambios pero es una actualización
            if (cambios.Length == "CAMBIOS REALIZADOS:\n".Length)
                return "REGISTRO ACTUALIZADO (sin cambios detectados)";

            return cambios.ToString();
        }


        private T DeserializarDeXml<T>(string xml) where T : class
        {
            if (string.IsNullOrEmpty(xml)) return null;

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var reader = new StringReader(xml))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al deserializar XML: {ex.Message}");
                return null;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            if (e.AddedItems[0] is HistorialDTO historial)
            {
                try
                {
                    // 1. Actualizar el historial seleccionado
                    HistorialSeleccionado = historial;

                    // 2. Decodificar los XML
                    var articuloAntes = !string.IsNullOrEmpty(historial.antes)
                        ? DeserializarDeXml<ArticuloDTO>(historial.antes)
                        : null;

                    var articuloDespues = !string.IsNullOrEmpty(historial.despues)
                        ? DeserializarDeXml<ArticuloDTO>(historial.despues)
                        : null;

                    // 3. Mostrar información básica
                    txtFechaHora.Text = historial.fechaHora.ToString();
                    txtUsuario.Text = historial.Usuario;
                    txtTipoModificacion.Text = historial.accion;

                    // 4. Cargar imágenes
                    imgAntes.Source = articuloAntes?.Foto != null
                        ? new BitmapImage(new Uri(articuloAntes.Foto, UriKind.RelativeOrAbsolute))
                        : null;

                    imgDespues.Source = articuloDespues?.Foto != null
                        ? new BitmapImage(new Uri(articuloDespues.Foto, UriKind.RelativeOrAbsolute))
                        : null;

                    // 5. Mostrar datos textuales
                    txtAntes.Text = articuloAntes != null
                        ? $"Descripción: {articuloAntes.Descripcion}\nStock: {articuloAntes.Stock}"
                        : "No hay datos anteriores";

                    txtDespues.Text = articuloDespues != null
                        ? $"Descripción: {articuloDespues.Descripcion}\nStock: {articuloDespues.Stock}"
                        : "No hay datos posteriores";

                    // 6. Mostrar comparación detallada
                    txtMotivo.Text = CompararCambios(articuloAntes, articuloDespues);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al cargar detalles: {ex.Message}");
                    txtMotivo.Text = "Error al cargar los detalles del historial";
                }
                txtNoImagenAntes.Visibility = imgAntes.Source == null ? Visibility.Visible : Visibility.Collapsed;
                txtNoImagenDespues.Visibility = imgDespues.Source == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }



    }
}
