using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDW_PDV_WPF.Controlador
{
    class ApiService
    {

        private readonly HttpClient _httpClient;

        public ApiService()
        {
            string apiUrl = Environment.GetEnvironmentVariable("API_OMEGA");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl) // Reemplaza con la URL real
            };
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonResponse);
            }

            return default;
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

            return response.IsSuccessStatusCode;  // Devuelve true si la respuesta es exitosa (status code 2xx)
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutAsync(string url, object data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Manejo de error
                MessageBox.Show($"Error al actualizar el artículo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }




    }


}
