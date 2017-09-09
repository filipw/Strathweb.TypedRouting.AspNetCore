# Strathweb.TypedRouting.AspNetCore

A library enabling strongly typed routing in ASP.NET Core MVC projects.

## Installation

Everything is on [Nuget](https://www.nuget.org/packages/Strathweb.TypedRouting.AspNetCore). [![Nuget](http://img.shields.io/nuget/v/Strathweb.TypedRouting.AspNetCore.svg?maxAge=10800)](https://www.nuget.org/packages/Strathweb.TypedRouting.AspNetCore)

```
nuget install Strathweb.TypedRouting.AspNetCore
```

## Setup

In your `Startup` class, after adding MVC, call `services.EnableTypedRouting();` and then configure your routes:

```csharp
        services.AddMvc(opt =>
        {
            opt.Get("homepage", c => c.Action<HomeController>(x => x.Index()));
            opt.Get("aboutpage/{name}", c => c.Action<HomeController>(x => x.About(Param<string>.Any)));
            opt.Post("sendcontact", c => c.Action<HomeController>(x => x.Contact()));
        }).EnableTypedRouting();
```

This creates:
* a GET route to `/homepage`
* a GET route to `/aboutpage/{name}`
* a POST route to `/sendcontact`

All of which go against the relevant methods on our `HomeController`.
The old API supported calling `.EnableTypedRouting();` on the `MvcOptions` too, but this approach is now deprecated.

## Link generation

Since the API is fluent, you can also give the routes names so that you can use them with i.e. link generation.

```csharp
opt.Get("api/values/{id}", c => c.Action<ValuesController>(x => x.Get(Param<int>.Any))).WithName("GetValueById");
```

Now you can use it with `IUrlHelper` (it's a `Url` property on every controller):

```csharp
var link = Url.Link("GetValueById", new { id = 1 });
```

`IUrlHelper` can also be obtained from `HttpContext`, anywhere in the pipeline (i.e. in a filter):

```csharp
var services = context.HttpContext.RequestServices;
var urlHelper = services.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(context);
var link = urlHelper.Link("GetValueById", new { id = 1 });
```

Finally, you can also use this link generation technique with the built-in action results, such as for example `CreatedAtRouteResult`:

```csharp
        public IActionResult Post([FromBody]string value)
        {
            var result = CreatedAtRoute("GetValueById", new { id = 1 }, "foo");
            return result;
        }
```

## Filters

The route definitions can also be done along with filters that should be executed for a given route. This is equivalent to defining a controller action, and annotating it with a relevant attribute such as action filter or authorization filter.

```csharp
services.AddMvc(opt =>
{
        opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).WithFilters(new AnnotationFilter());
}).EnableTypedRouting();
```

Filters can also be resolved from ASP.NET Core DI system - as long as they are registered there before.

```csharp
services.AddSingleton<TimerFilter>();

services.AddMvc(opt =>
{
        opt.Get("api/items", c => c.Action<ItemsController>(x => x.Get())).WithFilters(new AnnotationFilter());
}).EnableTypedRouting();
```

## Authorization Policies

The route definitions can also have ASP.NET Core authorization policies attached to them.

You can pass in a policy instance:

```csharp
services.AddMvc(opt =>
{
        opt.Get("api/secure", c => c.Action<OtherController>(x => x.Foo()).
                WithAuthorizationPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));
}).EnableTypedRouting();
```

You can also define a policy as string - then a corresponding policy must be previously registerd in ASP.NET Core DI system.

```csharp
services.AddAuthorization(o =>
{
        o.AddPolicy("MyPolicy", b => b.RequireAuthenticatedUser());
});

services.AddMvc(opt =>
{
        opt.Get("api/secure", c => c.Action<OtherController>(x => x.Foo()).
                WithAuthorizationPolicy("MyPolicy"));
}).EnableTypedRouting();
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
