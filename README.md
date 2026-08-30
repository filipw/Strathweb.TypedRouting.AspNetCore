# Strathweb.TypedRouting.AspNetCore

A library enabling strongly typed routing in ASP.NET Core MVC projects.

Supported on .NET 8 and .NET 10.

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

In your `Startup` class, after adding MVC, call `AddTypedRouting();` and then configure your routes:

```csharp
services.AddMvc().AddTypedRouting(opt =>
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

Parameters that can never appear in a URL - anything bound from the body, form, headers, or DI, plus
`CancellationToken` - are left out automatically:

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
services.AddMvc().AddTypedRouting(opt =>
{
    opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).WithFilters(new AnnotationFilter());
});
```

Filters can also be resolved from ASP.NET Core DI system - as long as they are registered there before.

```csharp
services.AddSingleton<TimerFilter>();

services.AddMvc().AddTypedRouting(opt =>
{
    opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).WithFilter<TimerFilter>();
});
```

## Authorization Policies

The route definitions can also have ASP.NET Core authorization policies attached to them.

You can pass in a policy instance:

```csharp
services.AddMvc().AddTypedRouting(opt =>
{
        opt.Get("api/secure", c => c.Action<OtherController>(x => x.Foo()).
                WithAuthorizationPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
});
```

You can also define a policy as string - then a corresponding policy must be previously registerd in ASP.NET Core DI system.

```csharp
services.AddAuthorization(o =>
{
        o.AddPolicy("MyPolicy", b => b.RequireAuthenticatedUser());
});

services.AddMvc().AddTypedRouting(opt =>
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

## License

[MIT](https://github.com/filipw/Strathweb.TypedRouting.AspNetCore/blob/master/LICENSE)
