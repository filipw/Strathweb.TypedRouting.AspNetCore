# Strathweb.TypedRouting.AspNetCore

A project enabling strongly typed routing in ASP.NET Core MVC projects.

## Installation

Everything is on Nuget - as prerelease. ASP.NET Core itself is prerelease and Nuget doesn't allow stable packages to be released with prerelease dependencies. As soon as ASP.NET Core ships RTM, I'll update this package to stable too.

```
nuget install Strathweb.TypedRouting.AspNetCore -pre
```

## Setup

In your `Startup` class, after adding MVC, call `opt.EnableTypedRouting();` and then configure your routes:

```csharp
        services.AddMvc();

        services.Configure<MvcOptions>(opt =>
        {
            opt.EnableTypedRouting();
            opt.Get("homepage", c => c.Action<HomeController>(x => x.Index()));
            opt.Get("aboutpage/{name}", c => c.Action<HomeController>(x => x.About(Param<string>.Any)));
            opt.Post("sendcontact", c => c.Action<HomeController>(x => x.Contact()));
        });
```

This creates:
* a GET route to `/homepage`
* a GET route to `/aboutpage/{name}`
* a POST route to `/sendcontact`

All of which go against the relevant methods on our `HomeController`.

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

## Route constraints

The library supports two ways of specifying route constraints:

 - inline in the template
 - via fluent API

The inline constraints are the same as you can use with attribute routing. For example:

```csharp
opt.Get("api/other/{id:int}", c => c.Action<OtherController>(x => x.Action2(Param<int>.Any)));
```

You can also specify constraints via the fluent API, by chaining `IActionConstraintMetadata` implementations. Consider the following sample constraint class:

```csharp
    public class ManadatoryHeaderConstraint : IActionConstraint, IActionConstraintMetadata
    {
        private string _header;

        public ManadatoryHeaderConstraint(string header)
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
opt.Get("api/other", c => c.Action<OtherController>(x => x.Action1())).WithConstraints(new ManadatoryHeaderConstraint("CustomHeader"));
```
