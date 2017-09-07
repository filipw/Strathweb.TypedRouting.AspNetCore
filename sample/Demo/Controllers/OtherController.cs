using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Controllers
{
    public class OtherController : Controller
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Await.Warning", "CS4014:Await.Warning")]
        public async Task<string> Action1()
        {
            await Task.Delay(100);
            return "bar";
        }

        public int Action2(int id)
        {
            return id;
        }

        public void Unreachable() { }

        public void Unreachable2() { }
    }
}
