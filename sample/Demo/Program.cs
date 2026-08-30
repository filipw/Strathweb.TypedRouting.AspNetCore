using Demo;
using Demo.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Strathweb.TypedRouting.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

    opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));

    opt.Get("links/named", c => c.Action<LinksController>(x => x.ToNamedRoute()));
    opt.Get("links/unnamed", c => c.Action<LinksController>(x => x.ToUnnamedRoute()));
    opt.Get("links/overload", c => c.Action<LinksController>(x => x.ToOverload()));
    opt.Get("links/extra-values", c => c.Action<LinksController>(x => x.WithExtraValues()));
    opt.Get("links/from-local/{id}", c => c.Action<LinksController>(x => x.FromLocal(Param<int>.Any)));
    opt.Get("links/async", c => c.Action<LinksController>(x => x.ToAsyncAction()));
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

app.Run();

// exposed so the integration tests can bootstrap the app via WebApplicationFactory
public partial class Program;
