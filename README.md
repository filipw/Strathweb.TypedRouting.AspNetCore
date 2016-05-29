# Strathweb.TypedRouting.AspNetCore

A library enabling strongly typed routing in ASP.NET Core MVC projects.

# Installation

```
Install-Package Strathweb.TypedRouting.AspNetCore -Pre
```

(Library is in pre-release at NuGet, because the whole ASP.NET Core MVC is RC2 at the moment)

# Usage

```

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

All of which go against the relevant methods on our `HomeController`. Since the API is fluent, you can also give the routes names so that you can use them with i.e. link generation.

```
opt.Get("api/values/{id}", c => c.Action<ValuesController>(x => x.Get(Param<int>.Any))).WithName("GetValueById");
```

Now you can use it with `IUrlHelper` (it's a `Url` property on every controller):

```
var link = Url.Link("GetValueById", new { id = 1 });
```

`IUrlHelper` can also be obtained from `HttpContext`, anywhere in the pipeline (i.e. in a filter):

```
var services = context.HttpContext.RequestServices;
var urlHelper = services.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(context);
var link = urlHelper.Link("GetValueById", new { id = 1 });
```

Finally, you can also use this link generation technique with the built-in action results, such as for example `CreatedAtRouteResult`:

```
        public IActionResult Post([FromBody]string value)
        {
            var result = CreatedAtRoute("GetValueById", new { id = 1 }, "foo");
            return result;
        }
```
