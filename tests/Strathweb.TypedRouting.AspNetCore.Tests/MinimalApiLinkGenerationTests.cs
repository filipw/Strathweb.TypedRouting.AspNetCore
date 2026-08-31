using Demo;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Strathweb.TypedRouting.AspNetCore.Tests
{
    public class MinimalApiLinkGenerationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _server;

        public MinimalApiLinkGenerationTests(WebApplicationFactory<Program> server)
        {
            _server = server;
        }

        [Fact]
        public async Task Generates_Links_To_Minimal_Api_Handlers()
        {
            var links = (await _server.CreateClient().GetStringAsync("minimal/links")).Split('\n');

            // named endpoint
            Assert.Equal("/minimal/items/5", links[0]);

            // unnamed endpoint, one route parameter and one query parameter
            Assert.Equal("/minimal/search/shoes?page=3", links[1]);

            // async handler, referenced through its Task returning signature
            Assert.Equal("/minimal/async/9", links[2]);

            // the body and the injected service are dropped, the route parameter survives
            Assert.Equal("/minimal/tenants/42/items", links[3]);

            // absolute URI
            Assert.Equal("http://localhost/minimal/items/5", links[4]);

            // caller supplied extras still reach the query string
            Assert.Equal("/minimal/items/5?debug=True", links[5]);
        }

        [Fact]
        public async Task CreatedAtHandler_Points_At_An_Unnamed_Handler()
        {
            var result = await _server.CreateClient().PostAsJsonAsync("minimal/items", new Item { Text = "x" });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal("http://localhost/minimal/items/77", result.Headers.Location?.ToString());
        }

        [Fact]
        public async Task Links_Cross_Between_Controllers_And_Minimal_Apis()
        {
            var client = _server.CreateClient();

            // minimal API endpoint generating a link to a controller action
            Assert.Equal("/api/items/3", await client.GetStringAsync("minimal/to-controller"));

            // controller generating a link to a minimal API handler
            Assert.Equal("/minimal/items/5", await client.GetStringAsync("links/to-minimal"));
        }

        [Fact]
        public async Task Handler_That_Was_Never_Mapped_Yields_No_Link()
        {
            // matches LinkGenerator semantics - an unroutable target produces null rather than throwing.
            // this is the case a Roslyn analyzer would catch at compile time instead
            Assert.Equal("<null>", await _server.CreateClient().GetStringAsync("minimal/unmapped-target"));
        }

        [Fact]
        public async Task Minimal_Api_Endpoints_Still_Work()
        {
            Assert.Equal("shoes/3", await _server.CreateClient().GetStringAsync("minimal/search/shoes?page=3"));
        }
    }
}
