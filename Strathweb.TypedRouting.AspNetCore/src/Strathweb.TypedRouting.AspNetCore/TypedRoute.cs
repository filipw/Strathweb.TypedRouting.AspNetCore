using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Strathweb.TypedRouting.AspNetCore
{
    public class TypedRoute : AttributeRouteModel
    {
        public TypedRoute(string template)
        {
            Template = template;
            HttpMethods = new string[0];
        }

        public TypeInfo ControllerType { get; private set; }

        public MethodInfo ActionMember { get; private set; }

        public IEnumerable<string> HttpMethods { get; private set; }

        public TypedRoute Controller<TController>()
        {
            ControllerType = typeof(TController).GetTypeInfo();
            return this;
        }

        public TypedRoute Action<T, U>(Expression<Func<T, U>> expression)
        {
            ActionMember = GetMethodInfoInternal(expression);
            ControllerType = ActionMember.DeclaringType.GetTypeInfo();
            return this;
        }

        public TypedRoute Action<T>(Expression<Action<T>> expression)
        {
            ActionMember = GetMethodInfoInternal(expression);
            ControllerType = ActionMember.DeclaringType.GetTypeInfo();
            return this;
        }

        private static MethodInfo GetMethodInfoInternal(dynamic expression)
        {
            var method = expression.Body as MethodCallExpression;
            if (method != null)
                return method.Method;

            throw new ArgumentException("Expression is incorrect!");
        }

        public TypedRoute WithName(string name)
        {
            Name = name;
            return this;
        }

        public TypedRoute ForHttpMethods(params string[] methods)
        {
            HttpMethods = methods;
            return this;
        }
    }
}
