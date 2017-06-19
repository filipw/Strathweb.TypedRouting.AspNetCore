using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo
{

    public class MandatoryHeaderConstraint : IActionConstraint, IActionConstraintMetadata
    {
        private string _header;

        public MandatoryHeaderConstraint(string header)
        {
            _header = header;
        }

        public int Order => 0;

        public bool Accept(ActionConstraintContext context)
        {
            if (context.RouteContext.HttpContext.Request.Headers.ContainsKey(_header))
            {
                return true;
            }

            return false;
        }
    }
}
