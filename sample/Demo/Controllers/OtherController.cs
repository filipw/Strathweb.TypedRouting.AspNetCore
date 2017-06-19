using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Controllers
{
    public class OtherController : Controller
    {
        public string Action1()
        {
            return "bar";
        }

        public int Action2(int id)
        {
            return id;
        }
    }
}
