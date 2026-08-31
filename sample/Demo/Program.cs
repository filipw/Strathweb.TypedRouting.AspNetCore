using Demo;
using Demo.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Strathweb.TypedRouting.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTypedLinkGeneration();
builder.Services.AddSingleton<TimerFilter>();
builder.Services.AddSingleton<AnnotationFilter>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o => o.RequireHttpsMetadata = false);

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("MyPolicy", b => b.RequireAuthenticatedUser());
});

builder.Services.AddControllers().AddTypedRouting(opt =>
{
    opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).
        WithFilters(new AnnotationFilter());

    opt.Get("api/items/{id}", c => c.Action<ItemsController>(x => x.Get(Param<int>.Any))).
        WithName("GetItemById").
        WithFilter<AnnotationFilter>();

    opt.Post("api/items", c => c.Action<ItemsController>(x => x.Post(Param<Item>.Any)));
    opt.Put("api/items/{id}", c => c.Action<ItemsController>(x => x.Put(Param<int>.Any, Param<Item>.Any)));
    opt.Delete("api/items/{id}", c => c.Action<ItemsController>(x => x.Delete(Param<int>.Any)));

    opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).
        WithConstraints(new MandatoryHeaderConstraint("CustomHeader"));

    // the same constraint as above, without needing a class for it
    opt.Get("api/other-lambda", c => c.Action<OtherController>(x => x.Action3())).
        WithConstraint(ctx => ctx.RouteContext.HttpContext.Request.Headers.ContainsKey("CustomHeader"));

    opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));

    opt.Get("links/named", c => c.Action<LinksController>(x => x.ToNamedRoute()));
    opt.Get("links/unnamed", c => c.Action<LinksController>(x => x.ToUnnamedRoute()));
    opt.Get("links/overload", c => c.Action<LinksController>(x => x.ToOverload()));
    opt.Get("links/extra-values", c => c.Action<LinksController>(x => x.WithExtraValues()));
    opt.Get("links/from-local/{id}", c => c.Action<LinksController>(x => x.FromLocal(Param<int>.Any)));
    opt.Get("links/async", c => c.Action<LinksController>(x => x.ToAsyncAction()));
    opt.Get("links/attribute-routed", c => c.Action<LinksController>(x => x.ToAttributeRouted()));
    opt.Get("links/attribute-routed-unnamed", c => c.Action<LinksController>(x => x.ToAttributeRoutedUnnamed()));
    opt.Get("links/to-minimal", c => c.Action<LinksController>(x => x.ToMinimalApi()));
    opt.Get("links/area", c => c.Action<LinksController>(x => x.ToArea()));
    opt.Get("links/by-methodinfo", c => c.Action<LinksController>(x => x.ByMethodInfo()));
    opt.Get("links/generator", c => c.Action<LinksController>(x => x.ViaLinkGenerator()));
    opt.Get("links/absolute", c => c.Action<LinksController>(x => x.AbsoluteUri()));

    opt.Get("api/secure_string", c => c.Action<OtherController>(x => x.Unreachable()).
        WithAuthorizationPolicy("MyPolicy"));

    opt.Get("api/secure_instance", c => c.Action<OtherController>(x => x.Unreachable()).
        WithAuthorizationPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// minimal API endpoints - deliberately a mix of named and unnamed
app.MapGet("/minimal/items/{id}", MinimalHandlers.GetItem).WithName("MinimalGetItem");
app.MapGet("/minimal/search/{q}", MinimalHandlers.Search);
app.MapGet("/minimal/async/{id}", MinimalHandlers.GetAsync);
app.MapPost("/minimal/tenants/{tenantId}/items", MinimalHandlers.Create);

// a typed Created result - the target endpoint does not need a name
app.MapPost("/minimal/items", (Item item) =>
    Results.Extensions.CreatedAtHandler(() => MinimalHandlers.GetItem(77), item));

// minimal API -> controller action
app.MapGet("/minimal/to-controller", (LinkGenerator links, HttpContext ctx) =>
    links.GetPathByAction<ItemsController>(ctx, x => x.Get(3)));

app.MapGet("/minimal/unmapped-target", (LinkGenerator links, HttpContext ctx) =>
    links.GetPathByHandler(ctx, () => MinimalHandlers.Unmapped(1)) ?? "<null>");

app.MapGet("/minimal/links", (LinkGenerator links, HttpContext ctx) => string.Join("\n", new[]
{
    links.GetPathByHandler(ctx, () => MinimalHandlers.GetItem(5)),
    links.GetPathByHandler(ctx, () => MinimalHandlers.Search("shoes", 3)),
    links.GetPathByHandler(ctx, () => MinimalHandlers.GetAsync(9)),
    links.GetPathByHandler(ctx, () => MinimalHandlers.Create(42, null!, null!)),
    links.GetUriByHandler(ctx, () => MinimalHandlers.GetItem(5)),
    links.GetPathByHandler(ctx, () => MinimalHandlers.GetItem(5), new { debug = true }),
}));

app.Run();

// exposed so the integration tests can bootstrap the app via WebApplicationFactory
public partial class Program;
