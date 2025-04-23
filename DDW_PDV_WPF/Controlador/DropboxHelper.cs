using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDW_PDV_WPF.Controlador
{
    public class DropboxHelper
    {
        private string AppKey = Properties.Settings.Default.APP_KEY;  
        private string AppSecret = Properties.Settings.Default.APP_SECRET; 
        private string DropboxToken = Properties.Settings.Default.DROPBOX_TOKEN; // Token de acceso de Dropbox



        // INTERNO DE LA CLASE
        public async Task<string> GetAccessToken()
        {
            // Specify the parameter type explicitly to resolve ambiguity
            var authUri = DropboxOAuth2Helper.GetAuthorizeUri(
                Dropbox.Api.OAuthResponseType.Code,
                AppKey,
                (string)null // Explicitly pass null as a string to resolve ambiguity
            );

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = authUri.ToString()
            });

            // Redirige al usuario a este URI para que se autentique
            Console.WriteLine("Abre esta URL en tu navegador para autorizar la aplicación:");
            Console.WriteLine(authUri);

            // Después de que el usuario autorice la aplicación, se te redirigirá con un código
            Console.WriteLine("Introduce el código de autorización:");
            var authCode = "";
            try
            {
                var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync("", AppKey, AppSecret);
                return response.AccessToken;  // Esto es lo que usarás para autenticarte con la API

            }
            catch (Exception ex) { }

            return null;

        }

        // INTERNO DE LA CLASE
        public async Task UploadFile(string filePath, string dropboxPath, string accessToken)
        {
            using (var dbx = new DropboxClient(accessToken))
            {
                var file = System.IO.File.OpenRead(filePath);

                var uploadResponse = await dbx.Files.UploadAsync(
                    dropboxPath,
                    WriteMode.Overwrite.Instance, // Si el archivo ya existe, lo sobrescribe
                    body: file);

                Console.WriteLine($"Archivo subido con éxito: {uploadResponse.PathDisplay}");
            }
        }


        // Método para obtener un enlace temporal al archivo subido
        public async Task<string> GetFileLink(string dropboxPath, string accessToken)
        {
            using (var dbx = new DropboxClient(accessToken))
            {
                var link = await dbx.Files.GetTemporaryLinkAsync(dropboxPath);
                return link.Link;  // Devuelve la URL temporal del archivo
            }
        }


        // ESTE ES EL METODO QUE SE USARA AL MOMENTO DE GUARDAR LOS ARTICULOS
        public async Task<string> UploadImageAndSaveUrl(string filePath)
        {
            var dropboxHelper = new DropboxHelper();

            // Sube el archivo a Dropbox
            string dropboxPath = "/" + System.IO.Path.GetFileName(filePath);
            await UploadFile(filePath, dropboxPath, DropboxToken);

            // Obtén la URL pública del archivo subido
            string fileUrl = await GetFileLink(dropboxPath, DropboxToken);

            // Guarda la URL en la base de datos
            return fileUrl; // Aquí puedes guardar la URL en tu base de datos o hacer lo que necesites con ella

        }


    }
}
