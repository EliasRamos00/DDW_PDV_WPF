using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Xml.Linq;
using System.Diagnostics;


namespace DDW_PDV_WPF
{



    public partial class frmInventarios : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly ApiService _apiService;
        private ObservableCollection<ArticuloDTO> _listaArticulos;
        private ObservableCollection<ArticuloDTO> _todosLosArticulos;
        private ArticuloDTO _articuloSeleccionado;
        private bool _hasChanges;
        private string _textoBusqueda;
        private bool _isNewItem = false;

        public ObservableCollection<ArticuloDTO> ListaArticulos
        {
            get => _listaArticulos;
            set
            {
                _listaArticulos = value;
                OnPropertyChanged(nameof(ListaArticulos));
            }
        }

        public ArticuloDTO ArticuloSeleccionado
        {
            get => _articuloSeleccionado;
            set
            {
                _articuloSeleccionado = value;
                btnCancelarCambios.Visibility = Visibility.Hidden;
                btnGuardarCambios.Visibility = Visibility.Hidden;
                OnPropertyChanged(nameof(ArticuloSeleccionado));
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                _hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));
                FiltrarArticulos();
            }
        }

        public ICommand LimpiarBusquedaCommand => new RelayCommand(LimpiarBusqueda);

        public frmInventarios()
        {
            InitializeComponent();
            DataContext = this;
            _apiService = new ApiService();
            CargarDatos();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LimpiarBusqueda() => TextoBusqueda = string.Empty;

        private async void CargarDatos()
        {
            var resultado = await _apiService.GetAsync<List<ArticuloDTO>>("/api/CArticulos");
            if (resultado != null)
            {
                _todosLosArticulos = new ObservableCollection<ArticuloDTO>(resultado);
                ListaArticulos = new ObservableCollection<ArticuloDTO>(resultado);
            }
        }

        private void FiltrarArticulos()
        {
            if (_todosLosArticulos == null) return;

            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                ListaArticulos = new ObservableCollection<ArticuloDTO>(_todosLosArticulos);
                return;
            }

            var texto = TextoBusqueda.ToLower();
            var resultados = _todosLosArticulos
                .Where(a => (a.Descripcion?.ToLower().Contains(texto) ?? false) ||
                           a.IdCategoria.ToString().Contains(texto) ||
                           a.idArticulo.ToString().Contains(texto) ||
                           (a.Color?.ToLower().Contains(texto) ?? false))
                .ToList();

            ListaArticulos = new ObservableCollection<ArticuloDTO>(resultados);
        }

        private void BotonAgregar(object sender, RoutedEventArgs e)
        {
            ArticuloSeleccionado = new ArticuloDTO
            {
                Stock = 0,
                Min = 0,
                Max = 0,
                PrecioVenta = 0,
                PrecioCompra = 0
            };

            _isNewItem = true;
            btnCancelarCambios.Visibility = Visibility.Visible;
            btnGuardarCambios.Visibility = Visibility.Visible;
            OnPropertyChanged(nameof(ArticuloSeleccionado));
        }

        private async void BotonQuitar(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null)
            {
                MessageBox.Show("Seleccione un artículo para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Eliminar {ArticuloSeleccionado.Descripcion}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes) return;

            var eliminado = await _apiService.DeleteAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}");

            if (eliminado)
            {
                ListaArticulos.Remove(ArticuloSeleccionado);
                MessageBox.Show("Artículo eliminado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Error al eliminar el artículo.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null || string.IsNullOrEmpty(ArticuloSeleccionado.Descripcion))
            {
                MessageBox.Show("Seleccione un artículo y complete la descripción.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                bool resultado = _isNewItem
                    ? await _apiService.PostAsync("/api/CArticulos", ArticuloSeleccionado)
                    : await _apiService.PutAsync($"/api/CArticulos/{ArticuloSeleccionado.idArticulo}", ArticuloSeleccionado);

                if (resultado)
                {
                    MessageBox.Show($"Artículo {(_isNewItem ? "creado" : "actualizado")} correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    _isNewItem = false;
                    CargarDatos();
                    btnCancelarCambios.Visibility = Visibility.Hidden;
                    btnGuardarCambios.Visibility = Visibility.Hidden;
                    HasChanges = false;
                }
                else
                {
                    MessageBox.Show("Error al guardar el artículo.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cambios: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewItem)
            {
                ArticuloSeleccionado = null;
                _isNewItem = false;
            }
            else if (ArticuloSeleccionado != null)
            {
                int idSeleccionado = ArticuloSeleccionado.idArticulo;
                CargarDatos();
                ArticuloSeleccionado = _todosLosArticulos.FirstOrDefault(a => a.idArticulo == idSeleccionado);
            }

            HasChanges = false;
            btnCancelarCambios.Visibility = Visibility.Hidden;
            btnGuardarCambios.Visibility = Visibility.Hidden;
        }

        private void BtnSeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null)
            {
                MessageBox.Show("Seleccione o cree un artículo primero.", "Advertencia",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Seleccionar imagen del artículo"
            };

            if (openFileDialog.ShowDialog() != true) return;

            try
            {
                string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string resourcesPath = System.IO.Path.Combine(projectRoot, "Resources");
                Directory.CreateDirectory(resourcesPath);

                string fileName = $"art_{DateTime.Now:yyyyMMddHHmmss}{System.IO.Path.GetExtension(openFileDialog.FileName)}";
                string finalPath = System.IO.Path.Combine(resourcesPath, fileName);

                File.Copy(openFileDialog.FileName, finalPath, true);
                ArticuloSeleccionado.Foto = $"Resources/{fileName}";

                OnPropertyChanged(nameof(ArticuloSeleccionado));
                HasChanges = true;
                btnCancelarCambios.Visibility = Visibility.Visible;
                btnGuardarCambios.Visibility = Visibility.Visible;
                AddFileToProject(System.IO.Path.Combine("Resources", fileName));

                MessageBox.Show("Imagen guardada correctamente", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la imagen: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEliminarImagen_Click(object sender, RoutedEventArgs e)
        {
            if (ArticuloSeleccionado == null || string.IsNullOrEmpty(ArticuloSeleccionado.Foto))
                return;

            var confirmacion = MessageBox.Show("¿Está seguro que desea eliminar la imagen de este artículo?",
                "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes) return;

            try
            {
                string fullPath = System.IO.Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName,
                    ArticuloSeleccionado.Foto);

                if (File.Exists(fullPath)) File.Delete(fullPath);

                ArticuloSeleccionado.Foto = null;
                OnPropertyChanged(nameof(ArticuloSeleccionado));
                HasChanges = true;
                btnCancelarCambios.Visibility = Visibility.Visible;
                btnGuardarCambios.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la imagen: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddFileToProject(string relativePath)
        {
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string projectFile = Directory.GetFiles(projectRoot, "*.csproj").FirstOrDefault();

            if (projectFile == null) return;

            try
            {
                var doc = XDocument.Load(projectFile);
                XNamespace ns = doc.Root.GetDefaultNamespace();

                if (!doc.Descendants(ns + "Content").Any(x => x.Attribute("Include")?.Value == relativePath))
                {
                    doc.Root.Add(new XElement(ns + "ItemGroup",
                        new XElement(ns + "Content",
                            new XAttribute("Include", relativePath),
                            new XElement(ns + "CopyToOutputDirectory", "PreserveNewest"))));
                    doc.Save(projectFile);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al añadir archivo al proyecto: {ex.Message}");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => HasChanges = true;
        private void txtDescripcion_MouseDoubleClick(object sender, MouseButtonEventArgs e) => HasChanges = true;

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
            public void Execute(object parameter) => _execute();

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }
        }
    }
}
