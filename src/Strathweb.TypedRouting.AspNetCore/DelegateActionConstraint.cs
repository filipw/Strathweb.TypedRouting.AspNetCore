using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Strathweb.TypedRouting.AspNetCore
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> backed by a delegate, so that simple constraints can be
    /// declared inline in the route definition instead of in a dedicated class.
    /// </summary>
    public sealed class DelegateActionConstraint : IActionConstraint
    {
        private readonly Func<ActionConstraintContext, bool> _constraint;

        public DelegateActionConstraint(Func<ActionConstraintContext, bool> constraint, int order = 0)
        {
            _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
            Order = order;
        }

        public int Order { get; }

        public bool Accept(ActionConstraintContext context) => _constraint(context);
    }
}
