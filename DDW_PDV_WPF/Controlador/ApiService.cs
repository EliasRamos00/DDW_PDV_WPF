using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
                return response.IsSuccessStatusCode;
            }
        }

    
}
