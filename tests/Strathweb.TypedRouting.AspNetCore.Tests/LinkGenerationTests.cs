using System.Net.Http.Json;
using Demo;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Strathweb.TypedRouting.AspNetCore.Tests
{
    public class LinkGenerationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _server;

        public LinkGenerationTests(WebApplicationFactory<Program> server)
        {
            _server = server;
        }

        private async Task<string> Get(string path)
        {
            var result = await _server.CreateClient().GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            return await result.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task Generates_Link_For_Named_Route()
        {
            Assert.Equal("/api/items/7", await Get("links/named"));
        }

        [Fact]
        public async Task Generates_Link_For_Unnamed_Route()
        {
            Assert.Equal("/api/other/42", await Get("links/unnamed"));
        }

        [Fact]
        public async Task Values_Not_In_The_Template_Become_Query_String()
        {
            Assert.Equal("/api/items/7?page=2", await Get("links/extra-values"));
        }

        [Fact]
        public async Task Generates_Link_From_A_Captured_Variable()
        {
            Assert.Equal("/api/items/13", await Get("links/from-local/13"));
        }

        [Fact]
        public async Task Generates_Link_For_Async_Action()
        {
            Assert.Equal("/api/other", await Get("links/async"));
        }

        [Fact]
        public async Task Generates_Link_Via_LinkGenerator()
        {
            Assert.Equal("/api/items/7", await Get("links/generator"));
        }

        [Fact]
        public async Task Generates_Absolute_Uri_Via_LinkGenerator()
        {
            Assert.Equal("http://localhost/api/items/7", await Get("links/absolute"));
        }

        [Fact]
        public async Task Overloaded_Action_Is_Disambiguated_By_The_Arguments()
        {
            // ItemsController.Get() and ItemsController.Get(int) share an action name.
            // x => x.Get() must produce the collection URL, not the by-id one
            Assert.Equal("/api/items", await Get("links/overload"));
            Assert.Equal("/api/items/7", await Get("links/named"));
        }

        [Fact]
        public async Task Generates_Links_To_Plain_Attribute_Routed_Actions()
        {
            // PlainController has no typed route registered - link generation still works,
            // which means the feature is usable in any controller based app
            Assert.Equal("/plain/3", await Get("links/attribute-routed"));
            Assert.Equal("/plain/unnamed/3", await Get("links/attribute-routed-unnamed"));
        }

        [Fact]
        public async Task Generates_Links_To_Actions_In_Areas()
        {
            Assert.Equal("/admin/reports/4", await Get("links/area"));
        }

        [Fact]
        public async Task Controller_Actions_Are_Addressable_By_MethodInfo()
        {
            Assert.Equal("/api/items/8", await Get("links/by-methodinfo"));
        }

        [Fact]
        public async Task CreatedAtAction_Sets_A_Typed_Location_Header()
        {
            var client = _server.CreateClient();
            var result = await client.PostAsJsonAsync("api/items", new Item { Text = "foo" });

            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.Equal("http://localhost/api/items/1", result.Headers.Location?.ToString());
        }

        [Fact]
        public async Task Body_Parameters_Are_Excluded_From_Generated_Links()
        {
            var client = _server.CreateClient();
            var result = await client.PostAsJsonAsync("api/items", new Item { Text = "foo" });

            // Url.Link<ItemsController>(x => x.Get(1)) - no trace of the posted Item
            Assert.Equal("http://localhost/api/items/1", Assert.Single(result.Headers.GetValues("TypedLink")));
        }
    }
}
