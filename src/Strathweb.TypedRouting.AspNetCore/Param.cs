using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Strathweb.TypedRouting.AspNetCore
{
    public static class Param<TValue>
    {
        public static TValue Any => default(TValue);
    }
}
