using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Demo.Areas.Admin.Controllers;
using System.Reflection;
using Strathweb.TypedRouting.AspNetCore;

namespace Demo.Controllers
{
    public class LinksController : Controller
    {
        private readonly LinkGenerator _linkGenerator;

        public LinksController(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        // route with a name - generation goes through the route name, which is exact
        public string ToNamedRoute() => Url.Action<ItemsController>(x => x.Get(7))!;

        // route without a name - generation falls back to action/controller matching
        public string ToUnnamedRoute() => Url.Action<OtherController>(x => x.Action2(42))!;

        // ItemsController.Get() and ItemsController.Get(int) share an action name - the
        // expression selects the overload, and the supplied arguments select the template
        public string ToOverload() => Url.Action<ItemsController>(x => x.Get())!;

        // extra values that are not route parameters end up in the query string
        public string WithExtraValues() => Url.Action<ItemsController>(x => x.Get(7), new { page = 2 })!;

        // the value can come from anywhere, not just a literal
        public string FromLocal(int id) => Url.Action<ItemsController>(x => x.Get(id))!;

        // an async action referenced through its Task returning signature
        public string ToAsyncAction() => Url.Action<OtherController>(x => x.Action1())!;

        // targets a controller that typed routing knows nothing about
        public string ToAttributeRouted() => Url.Action<PlainController>(x => x.ById(3))!;

        public string ToAttributeRoutedUnnamed() => Url.Action<PlainController>(x => x.Unnamed(3))!;

        // controller -> minimal API handler
        public string ToMinimalApi() => _linkGenerator.GetPathByHandler(HttpContext, () => MinimalHandlers.GetItem(5))!;

        // an action in an area
        public string ToArea() => Url.Action<ReportsController>(x => x.Get(4))!;

        // addressing a controller action by MethodInfo through the address scheme directly
        public string ByMethodInfo() =>
            _linkGenerator.GetPathByAddress(HttpContext,
                typeof(ItemsController).GetMethod(nameof(ItemsController.Get), new[] { typeof(int) })!,
                new Microsoft.AspNetCore.Routing.RouteValueDictionary(new { id = 8 }))!;

        public string ViaLinkGenerator() => _linkGenerator.GetPathByAction<ItemsController>(HttpContext, x => x.Get(7))!;

        public string AbsoluteUri() => _linkGenerator.GetUriByAction<ItemsController>(HttpContext, x => x.Get(7))!;
    }
}
