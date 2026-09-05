using Demo;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Strathweb.TypedRouting.AspNetCore.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly WebApplicationFactory<Program> _server;

        public IntegrationTests(WebApplicationFactory<Program> server)
        {
            _server = server;
        }

        [Fact]
        public async Task Get_List()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/items");
            var result = await client.SendAsync(request);
            var items = JsonSerializer.Deserialize<Item[]>(await result.Content.ReadAsStringAsync(), JsonOptions)!;

            Assert.Equal(2, items.Length);
            Assert.Equal("value1", items[0].Text);
            Assert.Equal("value2", items[1].Text);
        }

        [Fact]
        public async Task Filter_From_Instance()
        {
            var client = _server.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "api/items");
            var result = await client.SendAsync(request);

            Assert.Equal("Demo.AnnotationFilter", result.Headers.GetValues("FilterBefore").FirstOrDefault());
            Assert.Equal("Demo.AnnotationFilter", result.Headers.GetValues("FilterAfter").FirstOrDefault());
        }

        [Fact]
        public async Task Get_ById()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/items/7");
            var result = await client.SendAsync(request);
            var item = JsonSerializer.Deserialize<Item>(await result.Content.ReadAsStringAsync(), JsonOptions);

            Assert.NotNull(item);
            Assert.Equal("value", item.Text);
        }

        [Fact]
        public async Task Filter_From_DI()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/items/7");
            var result = await client.SendAsync(request);

            Assert.Equal("Demo.AnnotationFilter", result.Headers.GetValues("FilterBefore").FirstOrDefault());
            Assert.Equal("Demo.AnnotationFilter", result.Headers.GetValues("FilterAfter").FirstOrDefault());
        }

        [Fact]
        public async Task AuthorizationPolicy_DefineViaString()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/secure_string");
            var result = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AuthorizationPolicy_DefineViaPolicyInstance()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/secure_instance");
            var result = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Post()
        {
            var client = _server.CreateClient();

            var item = new Item { Text = "foo" };
            var request = new HttpRequestMessage(HttpMethod.Post, "api/items");
            request.Content = JsonContent.Create(item);

            var result = await client.SendAsync(request);
            var echoItem = JsonSerializer.Deserialize<Item>(await result.Content.ReadAsStringAsync(), JsonOptions);

            Assert.NotNull(echoItem);
            Assert.Equal(item.Text, echoItem.Text);
        }

        [Fact]
        public async Task Put()
        {
            var client = _server.CreateClient();

            var item = new Item { Text = "foo" };
            var request = new HttpRequestMessage(HttpMethod.Put, "api/items/10");
            request.Content = JsonContent.Create(item);

            var result = await client.SendAsync(request);
            var echoItem = JsonSerializer.Deserialize<Item>(await result.Content.ReadAsStringAsync(), JsonOptions);

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
        public async Task LambdaConstraint_Accepts_When_Satisfied()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/other-lambda");
            request.Headers.Add("CustomHeader", "abc");

            var result = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("lambda", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task LambdaConstraint_Rejects_When_Not_Satisfied()
        {
            var client = _server.CreateClient();

            var result = await client.GetAsync("api/other-lambda");

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task ApiOther_WithoutHeader()
        {
            var client = _server.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "api/other");
            var result = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }
    }
}
