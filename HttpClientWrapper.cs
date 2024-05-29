using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace articlesFreshServiceConnector.Data
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        public HttpClientWrapper()
        {
            
            _httpClient = new HttpClient();
        }

        public async Task<JsonElement.ArrayEnumerator> GetAsync(string apiUrl, string propName)
        {
            
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(configuration["AuthKeyAPI"])));

                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var Container = JsonSerializer.Deserialize<JsonDocument>(content);

                    var objArray = Container.RootElement.GetProperty(propName).EnumerateArray();
                    return objArray;

                }
                else
                {
                    Console.WriteLine($"Failed to fetch data. Status code: {response.StatusCode}");
                    return new JsonElement.ArrayEnumerator();
                }
            }
        }
}