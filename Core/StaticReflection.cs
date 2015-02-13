using System;
using System.Linq.Expressions;
using System.Reflection;

using Composite;

namespace CompositeC1Contrib
{
    public static class StaticReflection
    {
        public static MethodInfo GetGenericMethodInfo(Expression<Action<object>> expression)
        {
            Verify.ArgumentNotNull(expression, "expression");

            return GetMethodInfo(expression.Body).GetGenericMethodDefinition();
        }

        public static MethodInfo GetGenericMethodInfo(Expression<Func<object>> expression)
        {
            Verify.ArgumentNotNull(expression, "expression");

            return GetMethodInfo(expression.Body).GetGenericMethodDefinition();
        }

        public static MethodInfo GetMethodInfo<T, S>(Expression<Func<T, S>> expression)
        {
            return GetMethodInfo(expression.Body as MethodCallExpression);
        }

        public static MethodInfo GetMethodInfo<T>(Expression<Func<T>> expression)
        {
            return GetMethodInfo(expression.Body as MethodCallExpression);
        }

        public static MethodInfo GetMethodInfo(Expression expression)
        {
            Verify.ArgumentNotNull(expression, "expression");

            if (expression is UnaryExpression && expression.NodeType == ExpressionType.Convert)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            Verify.ArgumentCondition(expression is MethodCallExpression, "expressionBody", String.Format("Expression body should be of type '{0}'", typeof(MethodCallExpression).Name));

            return ((MethodCallExpression)expression).Method;
        }
    }
}
