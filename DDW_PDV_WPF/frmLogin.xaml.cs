using DDW_PDV_WPF.Controlador;
using DDW_PDV_WPF.Modelo;
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

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    public partial class frmLogin : Window
    {
        private readonly ApiService _apiService = new ApiService();

        public frmLogin()
        {
            InitializeComponent();
            btnIniciarSesion.IsEnabled = true;

            //string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DriveCache");

            //if (!Directory.Exists(cacheDirectory))
            //    return;

            //try
            //{
            //    Directory.Delete(cacheDirectory, true); // Elimina todo, incluyendo subdirectorios y archivos
            //    Directory.CreateDirectory(cacheDirectory); // La volvemos a crear vacía por si se sigue usando
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error al borrar la caché: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            // Inicializa el servicio de Google Drive

        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            btnIniciarSesion.IsEnabled = false;
            string usuario = txtUsuario.Text;
            string contrasena = ConvertirMD5(txtContrasena.Password); // Convertimos la contraseña ingresada a MD5

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
            {
                MessageBox.Show("Por favor ingrese usuario y contraseña");
                return;
            }

            try
            {
                var usuarioAutenticado = await AutenticarUsuario(usuario, contrasena);

                if (usuarioAutenticado != null)
                {
                    //MessageBox.Show($"Bienvenido {usuarioAutenticado.Usuario}"); SE REQUIERE AGREGAR ALGUNA LIBRERIA SENCILLA DE POPUPS PARA MOSTRAR MENSAJES
                    var VentanaPrincipal = new frmVentanaPrincipal(usuarioAutenticado.idUsuario.ToString(), usuarioAutenticado.Rol);
                    Properties.Settings.Default.idUsuario = usuarioAutenticado.idUsuario.ToString();
                    VentanaPrincipal.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos");
                    btnIniciarSesion.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al autenticar: {ex.Message}");
            }
        }

        private async Task<UsuarioDTO> AutenticarUsuario(string usuario, string contrasena)
        {
            var usuarios = await _apiService.GetAsync<List<UsuarioDTO>>("/api/CUsuarios/");

            if (usuarios != null)
            {
                return usuarios.Find(u => u.Usuario == usuario && u.Contra == contrasena);
            }

            return null;
        }

        private string ConvertirMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }

}
