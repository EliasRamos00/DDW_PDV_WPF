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
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
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
                    MessageBox.Show($"Bienvenido {usuarioAutenticado.Usuario}");
                    var VentanaPrincipal = new frmVentanaPrincipal();
                    VentanaPrincipal.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuario o contraseña incorrectos");
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
