# Strathweb.TypedRouting.AspNetCore

A library enabling strongly typed routing in ASP.NET Core MVC projects.

[![CI](https://github.com/filipw/Strathweb.TypedRouting.AspNetCore/actions/workflows/ci.yml/badge.svg)](https://github.com/filipw/Strathweb.TypedRouting.AspNetCore/actions/workflows/ci.yml)

Supported on .NET 8 and .NET 10.

## Feature support

What can be the target of a generated link:

| Link target | Works | Needs `WithName` / a route name |
| --- | --- | --- |
| Controller action routed by this library | yes | no |
| Controller action routed by `[HttpGet]` etc. | yes | no |
| Controller action in an area | yes | no |
| Minimal API endpoint, method group handler | yes | no |
| Minimal API endpoint, inline lambda handler | **no** | - |
| A method that was never mapped to an endpoint | returns `null` | - |

Inline lambda handlers cannot be link targets. A lambda compiles to a method with a generated name that
no other code can refer to, so there is nothing to put in the expression. Write the handler as a method
group instead.

Where links can be generated from, and with which API:

| From | Relative path | Absolute URI | Result pointing at another endpoint |
| --- | --- | --- | --- |
| Controller | `Url.Action<T>` | `Url.Link<T>` | `this.CreatedAtAction<T>`, `this.AcceptedAtAction<T>` |
| Minimal API handler | `GetPathByHandler` | `GetUriByHandler` | `Results.Extensions.CreatedAtHandler`, `AcceptedAtHandler` |
| Middleware, filters, anywhere with an `HttpContext` | `GetPathByAction<T>`, `GetPathByHandler` | `GetUriByAction<T>`, `GetUriByHandler` | - |

Controllers and minimal APIs can link to each other in either direction.

What typed routing adds over attribute routing, for controllers:

| | Typed routing | Attribute routing |
| --- | --- | --- |
| All routes declared in one place | yes | no |
| Renaming an action breaks the build | yes | no (silent 404) |
| Filters per route | `WithFilters`, `WithFilter<T>` | attributes |
| Authorization policy per route | `WithAuthorizationPolicy` | `[Authorize]` |
| Action constraints from a class | `WithConstraints` | attributes |
| Action constraints from a lambda | `WithConstraint` | not available |
| Inline template constraints (`{id:int}`) | yes | yes |
| Route template highlighting and analysis in the IDE | yes | yes |
| Route template checked against the action signature | no | no |

## Installation

Everything is on [Nuget](https://www.nuget.org/packages/Strathweb.TypedRouting.AspNetCore). [![Nuget](http://img.shields.io/nuget/v/Strathweb.TypedRouting.AspNetCore.svg?maxAge=10800)](https://www.nuget.org/packages/Strathweb.TypedRouting.AspNetCore)

```
nuget install Strathweb.TypedRouting.AspNetCore
```
or via the .NET Core CLI:

```
dotnet add package Strathweb.TypedRouting.AspNetCore
```

## Setup

After adding MVC, call `AddTypedRouting()` and configure your routes:

```csharp
builder.Services.AddControllers().AddTypedRouting(opt =>
{
    opt.Get("homepage", c => c.Action<HomeController>(x => x.Index()));
    opt.Get("aboutpage/{name}", c => c.Action<HomeController>(x => x.About(Param<string>.Any)));
    opt.Post("sendcontact", c => c.Action<HomeController>(x => x.Contact()));
});
```

This creates:
* a GET route to `/homepage`
* a GET route to `/aboutpage/{name}`
* a POST route to `/sendcontact`

All of which will route to the relevant methods on our `HomeController`.

## Link generation

Route definitions can be given names, so that they can be referenced from `IUrlHelper`:

```csharp
opt.Get("api/values/{id}", c => c.Action<ValuesController>(x => x.Get(Param<int>.Any))).WithName("GetValueById");
```

```csharp
var link = Url.Link("GetValueById", new { id = 1 });
```

### Typed link generation

Route names are still strings, so the library also lets you generate links from the action itself. Rename or
re-sign an action and you get a compile error instead of a broken URL at runtime:

```csharp
// "/api/values/1"
var path = Url.Action<ValuesController>(x => x.Get(1));

// "https://localhost:5001/api/values/1"
var uri = Url.Link<ValuesController>(x => x.Get(1));
```

Arguments are read straight from the expression, and they do not have to be literals:

```csharp
var path = Url.Action<ValuesController>(x => x.Get(item.Id));
```

Values that are not part of the route template are appended as a query string:

```csharp
// "/api/values/1?page=2"
var path = Url.Action<ValuesController>(x => x.Get(1), new { page = 2 });
```

Arguments are turned into URL parts according to what they are bound from:

| Argument | Ends up as |
| --- | --- |
| A parameter named in the route template | a path segment |
| Any other simple value (number, string, `Guid`, `DateTime`, enum, ...) | a query string value |
| `[FromBody]`, or a complex type with no binding source | left out |
| `[FromServices]`, `[FromHeader]`, `[FromForm]`, `IFormFile` | left out |
| `CancellationToken`, `HttpContext` and friends | left out |
| `Param<T>.Any` | left out |
| `null` | left out |

So a body parameter never leaks into the URL:

```csharp
// "/api/values/1", the posted model is ignored
var path = Url.Action<ValuesController>(x => x.Put(1, model));
```

Overloaded actions are disambiguated by the expression, so `x => x.Get()` and `x => x.Get(1)` generate links
to their own routes. When the target route was given a name via `WithName`, that name is used for generation,
which identifies the endpoint exactly.

### Outside of controllers

`LinkGenerator` has the same extensions, for use in middleware, filters and anywhere else with an
`HttpContext` at hand:

```csharp
var path = linkGenerator.GetPathByAction<ValuesController>(httpContext, x => x.Get(1));
var uri = linkGenerator.GetUriByAction<ValuesController>(httpContext, x => x.Get(1));
```

### Minimal APIs

Minimal API link generation is normally addressed by route name, which forces every link target to be
given a name. The same expression based API works there instead, against the handler itself.

Register the address scheme at startup:

```csharp
builder.Services.AddTypedLinkGeneration();
```

Handlers have to be written as method groups rather than inline lambdas - an inline lambda compiles to a
method that cannot be referenced from anywhere else, so it can never be a link target:

```csharp
app.MapGet("/items/{id}", ItemHandlers.GetById);
app.MapGet("/search/{q}", ItemHandlers.Search);
```

```csharp
// "/items/5", no WithName needed anywhere
var path = links.GetPathByHandler(httpContext, () => ItemHandlers.GetById(5));

// "https://localhost:5001/items/5"
var uri = links.GetUriByHandler(httpContext, () => ItemHandlers.GetById(5));

// route parameters and query parameters are told apart by the route template
// "/search/shoes?page=3"
var search = links.GetPathByHandler(httpContext, () => ItemHandlers.Search("shoes", 3));
```

There is a typed `Created` result too, which resolves the location during execution, so the handler does
not need an `HttpContext` parameter just to build a link:

```csharp
app.MapPost("/items", (Item item) =>
    Results.Extensions.CreatedAtHandler(() => ItemHandlers.GetById(77), item));
```

`AcceptedAtHandler` works the same way.

Links can be generated in either direction - a minimal API endpoint can link to a controller action with
`GetPathByAction<TController>`, and a controller can link to a minimal API handler with `GetPathByHandler`.

As with the framework's own link generation, a target that is not routable produces `null` rather than
throwing.

### Action results

The built-in action results that point at another action have typed counterparts too:

```csharp
public IActionResult Post([FromBody] Value value)
{
    return this.CreatedAtAction<ValuesController>(x => x.Get(1), value);
}
```

`AcceptedAtAction<T>` works the same way.

## Filters

The route definitions can also be done along with filters that should be executed for a given route. This is equivalent to defining a controller action, and annotating it with a relevant attribute such as action filter or authorization filter.

```csharp
builder.Services.AddControllers().AddTypedRouting(opt =>
{
    opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).WithFilters(new AnnotationFilter());
});
```

Filters can also be resolved from ASP.NET Core DI system - as long as they are registered there before.

```csharp
builder.Services.AddSingleton<TimerFilter>();

builder.Services.AddControllers().AddTypedRouting(opt =>
{
    opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).WithFilter<TimerFilter>();
});
```

## Authorization Policies

The route definitions can also have ASP.NET Core authorization policies attached to them.

You can pass in a policy instance:

```csharp
builder.Services.AddControllers().AddTypedRouting(opt =>
{
        opt.Get("api/secure", c => c.Action<OtherController>(x => x.Foo()).
                WithAuthorizationPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
});
```

You can also define a policy as string - then a corresponding policy must be previously registerd in ASP.NET Core DI system.

```csharp
builder.Services.AddAuthorization(o =>
{
        o.AddPolicy("MyPolicy", b => b.RequireAuthenticatedUser());
});

builder.Services.AddControllers().AddTypedRouting(opt =>
{
        opt.Get("api/secure", c => c.Action<OtherController>(x => x.Foo()).
                WithAuthorizationPolicy("MyPolicy"));
});
```

## Action constraints

The library supports two ways of specifying MVC action constraints:

 - inline in the template
 - via fluent API

The inline constraints are the same as you can use with attribute routing. For example:

```csharp
opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));
```

You can also specify constraints via the fluent API, by chaining `IActionConstraintMetadata` implementations. Consider the following sample constraint class:

```csharp
    public class MandatoryHeaderConstraint : IActionConstraint, IActionConstraintMetadata
    {
        private string _header;

        public MandatoryHeaderConstraint(string header)
        {
            _header = header;
        }

        public int Order
        {
            get
            {
                return 0;
            }
        }

        public bool Accept(ActionConstraintContext context)
        {
            // only allow route to be hit if the predefined header is present
            if (context.RouteContext.HttpContext.Request.Headers.ContainsKey(_header))
            {
                return true;
            }

            return false;
        }
    }
```

You can now use this class in the route declaration:

```csharp
opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).WithConstraints(new MandatoryHeaderConstraint("CustomHeader"));
```

For simple cases a class is unnecessary - a constraint can be declared inline as a lambda:

```csharp
opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).
    WithConstraint(ctx => ctx.RouteContext.HttpContext.Request.Headers.ContainsKey("CustomHeader"));
```

`WithConstraint` takes an optional `order` argument, matching `IActionConstraint.Order`.

## License

[MIT](https://github.com/filipw/Strathweb.TypedRouting.AspNetCore/blob/master/LICENSE)
