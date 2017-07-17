using Demo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
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

            var request = new HttpRequestMessage(HttpMethod.Get, "api/items");
            var result = await client.SendAsync(request);
            var items = JsonConvert.DeserializeObject<Item[]>(await result.Content.ReadAsStringAsync());

            Assert.Equal(2, items.Length);
            Assert.Equal("value1", items[0].Text);
            Assert.Equal("value2", items[1].Text);
        }

        [Fact]
        public async Task Get_ById()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/items/7");
            var result = await client.SendAsync(request);
            var item = JsonConvert.DeserializeObject<Item>(await result.Content.ReadAsStringAsync());

            Assert.NotNull(item);
            Assert.Equal("value", item.Text);
        }

        [Fact]
        public async Task Post()
        {
            var client = _server.CreateClient();

            var item = new Item { Text = "foo" };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/items");
            request.Content = new StringContent(JsonConvert.SerializeObject(item));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await client.SendAsync(request);
            var echoItem = JsonConvert.DeserializeObject<Item>(await result.Content.ReadAsStringAsync());

            Assert.NotNull(echoItem);
            Assert.Equal(item.Text, echoItem.Text);
        }

        [Fact]
        public async Task Put()
        {
            var client = _server.CreateClient();

            var item = new Item { Text = "foo" };
            var request = new HttpRequestMessage(HttpMethod.Put, "api/items/10");
            request.Content = new StringContent(JsonConvert.SerializeObject(item));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = await client.SendAsync(request);
            var echoItem = JsonConvert.DeserializeObject<Item>(await result.Content.ReadAsStringAsync());

            Assert.NotNull(echoItem);
            Assert.Equal(item.Text, echoItem.Text);
        }

        [Fact]
        public async Task Delete()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, "api/items/10");

            var result = await client.SendAsync(request);
            var response = await result.Content.ReadAsStringAsync();

            Assert.NotNull(response);
            Assert.Equal("10", response);
        }

        [Fact]
        public async Task ApiOther_WithHeader()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/other");
            request.Headers.Add("CustomHeader", "abc");

            var result = await client.SendAsync(request);
            var response = await result.Content.ReadAsStringAsync();

            Assert.NotNull(response);
            Assert.Equal("bar", response);
        }

        [Fact]
        public async Task ApiOther_WithoutHeader()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/other");
            var result = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task ApiOther_WithHeader_VerifyFilterPresence()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/other");
            request.Headers.Add("CustomHeader", "abc");

            var result = await client.SendAsync(request);
            var executionTime = result.Headers.GetValues("ActionDuration").FirstOrDefault();

            Assert.NotNull(executionTime);
        }
    }
}
