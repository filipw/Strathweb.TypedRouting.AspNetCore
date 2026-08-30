using System;
using System.Linq.Expressions;

namespace Strathweb.TypedRouting.AspNetCore
{
    internal static class ExpressionHelper
    {
        public static MethodCallExpression GetMethodCall(LambdaExpression expression)
        {
            var body = expression.Body;

            // unwrap the conversion the compiler inserts when a value-returning action
            // is bound to an Expression<Func<T, object>>
            if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            {
                body = unary.Operand;
            }

            if (body is MethodCallExpression method)
            {
                return method;
            }

            throw new ArgumentException("Expression is incorrect - it must be a single call to a controller action.", nameof(expression));
        }

        public static bool IsParamPlaceholder(Expression expression) =>
            expression is MemberExpression { Member.DeclaringType: { IsGenericType: true } declaringType } &&
            declaringType.GetGenericTypeDefinition() == typeof(Param<>);

        public static object? Evaluate(Expression expression)
        {
            switch (expression)
            {
                case ConstantExpression constant:
                    return constant.Value;

                // the common closure case - a local captured into a compiler generated display class
                case MemberExpression { Expression: ConstantExpression owner } member:
                    return member.Member switch
                    {
                        System.Reflection.FieldInfo field => field.GetValue(owner.Value),
                        System.Reflection.PropertyInfo property => property.GetValue(owner.Value),
                        _ => CompileAndInvoke(expression)
                    };

                default:
                    return CompileAndInvoke(expression);
            }
        }

        private static object? CompileAndInvoke(Expression expression) =>
            Expression.Lambda<Func<object?>>(Expression.Convert(expression, typeof(object))).Compile()();
    }
}
