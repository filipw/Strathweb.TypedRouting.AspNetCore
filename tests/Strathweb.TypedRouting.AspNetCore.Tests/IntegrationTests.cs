using Demo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Strathweb.TypedRouting.AspNetCore.Tests
{
    public class IntegrationTests
    {
        private TestServer _server;

        public IntegrationTests()
        {
            _server = new TestServer(new WebHostBuilder()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>());
        }

        [Fact]
        public async Task Get_List()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/values");
            var result = await client.SendAsync(request);
            var values = JsonConvert.DeserializeObject<string[]>(await result.Content.ReadAsStringAsync());

            Assert.Equal(2, values.Length);
            Assert.Equal("value1", values[0]);
            Assert.Equal("value2", values[1]);
        }

        [Fact]
        public async Task Get_ById()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/values/7");
            var result = await client.SendAsync(request);
            var value = await result.Content.ReadAsStringAsync();

            Assert.NotNull(value);
            Assert.Equal("value", value);
        }
    }
}
