using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace CCPV.Main.Test
{
    public class TestServer<TStartup> where TStartup : class
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly (Type type, object obj)[] _diObjects;

        public TestServer(
            ITestOutputHelper testOutputHelper,
            params (Type type, object obj)[] diObjects)
        {
            _testOutputHelper = testOutputHelper;
            _diObjects = diObjects;
        }

        public async Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string>? headers = null)
        {
            HttpResponseMessage? httpResponseMessage = null;
            await PerformTestRequestAsync(async httpClient =>
            {
                HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, uri);
                AppendHeaders(httpRequestMessage, headers);
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            });
            Debug.Assert(httpResponseMessage != null);
            return httpResponseMessage!;
        }

        public async Task<HttpResponseMessage> PostAsync(string uri, object body, Dictionary<string, string>? headers = null)
        {
            HttpResponseMessage? httpResponseMessage = null;
            await PerformTestRequestAsync(async httpClient =>
            {
                HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, uri);
                AppendHeaders(httpRequestMessage, headers);
                string content = JsonSerializer.Serialize(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });
                httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
                httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            });
            Debug.Assert(httpResponseMessage != null);
            return httpResponseMessage!;
        }

        public async Task PerformTestRequestAsync(Func<HttpClient, Task> testOperation, params (Type type, object obj)[] diObjects)
        {
            WebApplicationFactory<TStartup> factory = new WebApplicationFactory<TStartup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        foreach ((Type type, object obj) in _diObjects)
                        {
                            services.AddSingleton(type, obj);
                        }
                        foreach ((Type type, object obj) in diObjects)
                        {
                            services.AddSingleton(type, obj);
                        }
                    });
                });
            HttpClient client = factory.CreateClient();
            await testOperation(client);
        }

        private void AppendHeaders(HttpRequestMessage request, Dictionary<string, string>? headers)
        {
            if (headers == null) return;
            foreach (KeyValuePair<string, string> kv in headers)
            {
                request.Headers.Add(kv.Key, kv.Value);
            }
        }
    }
}
