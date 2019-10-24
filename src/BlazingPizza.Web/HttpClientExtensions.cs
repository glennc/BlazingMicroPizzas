using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazingPizza.Web
{
    //TODO: Sort out JSON Options
    public static class HttpClientExtensions
    {
        public static async Task<T> GetJsonAsync<T>(this HttpClient client, string uri)
        {
            var resp = await client.GetAsync(uri);
            resp.EnsureSuccessStatusCode();
            var data = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        public static async Task<T> PostJsonAsync<T>(this HttpClient client, string uri, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Version = HttpVersion.Version20,
                Content = new StringContent(JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), Encoding.UTF8, "application/json")
            };

            var resp = await client.SendAsync(request);
            resp.EnsureSuccessStatusCode();
            var stringContent = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(stringContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
