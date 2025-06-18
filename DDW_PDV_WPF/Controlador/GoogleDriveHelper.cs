using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using File = Google.Apis.Drive.v3.Data.File;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;



namespace DDW_PDV_WPF.Controlador
{


    public class GoogleDriveHelper
    {
        private static DriveService _service;

        /// <summary>
        /// Inicializa el servicio de Google Drive con una cuenta de servicio.
        /// </summary>
        /// <param name="jsonPath">Ruta al archivo de credenciales JSON descargado desde Google Cloud.</param>
        public static void Initialize(string jsonPath)
        {
            var credential = GoogleCredential.FromFile(jsonPath)
                .CreateScoped(DriveService.ScopeConstants.Drive);

            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FLORERIAOMEGA"
            });
        }

        /// <summary>
        /// Sube un archivo a una carpeta específica de Google Drive.
        /// </summary>
        /// <param name="filePath">Ruta del archivo local.</param>
        /// <param name="folderId">ID de la carpeta en Drive (compartida con la cuenta de servicio).</param>
        /// <returns>ID del archivo subido.</returns>
        public static async Task<string> UploadFileAsync(string filePath, string folderId)
        {
            if (_service == null)
                throw new InvalidOperationException("GoogleDriveService no ha sido inicializado.");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { folderId }
            };

            FilesResource.CreateMediaUpload request;

            string resizedPath = ResizeAndCompressImage(filePath, 800, 800); 

            using (var stream = new FileStream(resizedPath, FileMode.Open, FileAccess.Read))
            {
                string mimeType = GetMimeType(resizedPath);
                request = _service.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id";
                await request.UploadAsync();
            }
            return request.ResponseBody.Id;
        }

        /// <summary>
        /// Hace que un archivo sea público y retorna su enlace de visualización.
        /// </summary>
        /// <param name="fileId">ID del archivo en Google Drive.</param>
        /// <returns>Enlace público del archivo.</returns>
        public static async Task<string> GetPublicLinkAsync(string fileId)
        {
            if (_service == null)
                throw new InvalidOperationException("GoogleDriveService no ha sido inicializado.");

            var permission = new Permission
            {
                Role = "reader",
                Type = "anyone"
            };

            await _service.Permissions.Create(permission, fileId).ExecuteAsync();

            var file = await _service.Files.Get(fileId).ExecuteAsync();
            return file.WebViewLink;
        }

        /// <summary>
        /// Obtiene el tipo MIME según la extensión del archivo.
        /// </summary>
        private static string GetMimeType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();

            // Determina el tipo MIME según la extensión del archivo
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".pdf":
                    return "application/pdf";
                case ".txt":
                    return "text/plain";
                case ".doc":
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case ".xls":
                case ".xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ".ppt":
                case ".pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp4":
                    return "video/mp4";
                case ".zip":
                    return "application/zip";
                case ".rar":
                    return "application/x-rar-compressed";
                default:
                    return "application/octet-stream"; // Para archivos desconocidos
            }
        }


        // Método para consultar y guardar imágenes en caché
        public async Task<BitmapImage> GetImageFromCacheOrDownload(string imageLink, string imageId)
        {
            // Ruta de la carpeta de caché donde se almacenarán las imágenes descargadas
            string cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DriveCache");

            // Asegurarse de que la carpeta de caché existe, si no, la creamos
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            // Ruta del archivo de caché (lo almacenamos como .jpg, pero puede ser cualquier extensión de imagen)
            string cachedFilePath = Path.Combine(cacheDirectory, $"{imageId}.jpg");

            // Verificar si la imagen ya está en caché
            if (System.IO.File.Exists(cachedFilePath))
            {
                // Si la imagen ya está en caché, cargarla y devolverla
                return LoadImageFromFile(cachedFilePath);
            }

            // Si la imagen no está en caché, la descargamos
            return await DownloadAndCacheImage(imageLink, cachedFilePath);
        }

        // Método para cargar la imagen desde un archivo (para mostrarla en WPF)
        private BitmapImage LoadImageFromFile(string filePath)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Carga completamente y cierra el stream
                bitmapImage.DecodePixelWidth = 300; // Redimensiona si las imágenes son muy grandes
                bitmapImage.StreamSource = stream;
                bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Permite compartir en UI y libera recursos
            }

            return bitmapImage;
        }


        private static string ResizeAndCompressImage(string inputPath, int maxWidth = 800, int maxHeight = 800, long quality = 80)
        {
            using (var originalImage = System.Drawing.Image.FromFile(inputPath))
            {
                // Redimensionar la imagen a 800x800 píxeles
                int newWidth = maxWidth;
                int newHeight = maxHeight;

                using (var bitmap = new System.Drawing.Bitmap(originalImage, newWidth, newHeight))
                {
                    // Configuración para comprimir la imagen
                    var qualityParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    var codecInfo = GetEncoderInfo("image/jpeg");
                    var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                    encoderParams.Param[0] = qualityParam;

                    // Guardar la imagen comprimida
                    string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".jpg");
                    bitmap.Save(tempPath, codecInfo, encoderParams);

                    return tempPath;
                }
            }
        }

        // Método auxiliar para obtener el códec del JPEG
        private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var encoders = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (var encoder in encoders)
            {
                if (encoder.MimeType == mimeType)
                    return encoder;
            }
            return null;
        }




        public static async Task MakeFilePublicAsync(string fileId)
        {

            var permission = new Google.Apis.Drive.v3.Data.Permission()
            {
                Role = "reader",
                Type = "anyone"
            };

            await _service.Permissions.Create(permission, fileId).ExecuteAsync();
        }


        // Método para descargar la imagen y guardarla en caché
        private async Task<BitmapImage> DownloadAndCacheImage(string imageLink, string cachedFilePath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    // Descargar la imagen usando el link proporcionado
                    byte[] imageBytes = await client.GetByteArrayAsync(imageLink);

                    // Guardar la imagen descargada en el directorio de caché
                    using (var stream = new FileStream(cachedFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await stream.WriteAsync(imageBytes, 0, imageBytes.Length);
                    }

                    // Ahora que la imagen está en caché, cargarla y devolverla
                    return LoadImageFromFile(cachedFilePath);
                }
                catch (Exception ex)
                {
                    // Si ocurre algún error, manejarlo (puedes lanzar una excepción o devolver null)
                    //MessageBox.Show($"Error al descargar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }

        public async Task<string> GetPublicLinkForFile(DriveService driveService, string fileId)
        {
            var request = driveService.Files.Get(fileId);
            request.Fields = "webViewLink";  // Obtiene el enlace de vista previa del archivo

            var file = await request.ExecuteAsync();
            return file.WebViewLink;  // Este es el enlace público para ver el archivo
        }




    }




}
