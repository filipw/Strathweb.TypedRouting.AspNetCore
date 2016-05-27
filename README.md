# Strathweb.TypedRouting.AspNetCore

A project enabling strongly typed routing in ASP.NET Core MVC projects.

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
opt.GetRoute("homepage", c => c.Action<HomeController>(x => x.Index())).WithName("foo");
```
