using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using AuthService.Models;

namespace LiveTelemetrySensor.Common.Services.Network
{
    public class RequestsService
    {
        private HttpClient _httpClient;
        public RequestsService(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient(Constants.HTTP_CLIENT_NAME);
        }
        public async Task<string> PostAsync(string uri, object toSend)
        {
            StringContent requestContent = new StringContent(
                JsonSerializer.Serialize(toSend),
                Encoding.UTF8,
                "application/json"
            );
            HttpResponseMessage response = await _httpClient.PostAsync(uri, requestContent);
            return await handleResponseAsync(response);
        }

        public async Task<string> GetAsync(string uri)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(uri);
            return await handleResponseAsync(response);
        }

        public async Task<DeserializedClass?> GetAsync<DeserializedClass>(string uri) where DeserializedClass : class
        {
            string jsonResponse = await GetAsync(uri);
            return JsonSerializer.Deserialize<DeserializedClass>(jsonResponse);
        }
        public async Task<DeserializedClass?> PostAsync<DeserializedClass>(string uri, object toSend) where DeserializedClass : class
        {
            string jsonResponse = await PostAsync(uri, toSend);
            return JsonSerializer.Deserialize<DeserializedClass>(jsonResponse);
        }

        private async Task<string> handleResponseAsync(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }
    }
}
