using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;

namespace LiveTelemetrySensor.SensorAlerts.Services.Network
{
    public class RequestsService
    {
        private HttpClient _httpClient;
        public RequestsService()
        {
            _httpClient = new HttpClient();
        }
        public async Task<string> PostAsync(string uri, Object toSend)
        {
            StringContent requestContent = new StringContent(
                JsonSerializer.Serialize(toSend),
                Encoding.UTF8,
                "application/json"
            );
            HttpResponseMessage response = await _httpClient.PostAsync(uri, requestContent);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }
        public async Task<DeserializedClass> PostAsync<DeserializedClass>(string uri, Object toSend)
        {
            string jsonResponse = await PostAsync(uri, toSend);
            return JsonSerializer.Deserialize<DeserializedClass>(jsonResponse);
        }

    }
}
