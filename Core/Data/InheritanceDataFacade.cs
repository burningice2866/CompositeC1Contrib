using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Composite.Data;

namespace CompositeC1Contrib.Data
{
    public class InheritanceDataFacade
    {
        private static readonly MethodInfo CastMethod = typeof(Queryable).GetMethod("Cast", BindingFlags.Static | BindingFlags.Public);
        private static readonly MethodInfo DeleteMethod = GetDeleteMethod();

        private static readonly ConcurrentDictionary<Type, IList<Type>> InheritanceMap = new ConcurrentDictionary<Type, IList<Type>>();

        public static void Delete<T>(Expression<Func<T, bool>> whereSelector) where T : IData
        {
            var inheritedTypes = GetInheritedTypes<T>();

            foreach (var type in inheritedTypes)
            {
                var genericCast = CastMethod.MakeGenericMethod(type);
                var genericDelete = DeleteMethod.MakeGenericMethod(type);

                var items = DataFacade.GetData(type).Cast<T>().Where(whereSelector);
                var converted = (IQueryable)genericCast.Invoke(null, new object[] { items });

                genericDelete.Invoke(null, new object[] { converted });
            }
        }

        public static IEnumerable<T> GetData<T>(Expression<Func<T, bool>> whereSelector) where T : IData
        {
            var list = new List<T>();
            var inheritedTypes = GetInheritedTypes<T>();

            foreach (var type in inheritedTypes)
            {
                var items = DataFacade.GetData(type).Cast<T>().Where(whereSelector);

                list.AddRange(items);
            }

            return list;
        }

        private static MethodInfo GetDeleteMethod()
        {
            var methods = typeof(DataFacade).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(m => m.Name == "Delete");
            foreach (var method in methods)
            {
                var genericArguments = method.GetGenericArguments().ToList();
                if (genericArguments.Count != 1)
                {
                    continue;
                }

                var parameters = method.GetParameters().ToList();
                if (parameters.Count != 1)
                {
                    continue;
                }

                var parameter = parameters[0];
                if (!parameter.ParameterType.IsGenericType)
                {
                    continue;
                }

                var genericType = parameter.ParameterType.GetGenericTypeDefinition();
                if (genericType != typeof(IEnumerable<>))
                {
                    continue;
                }

                genericArguments = parameter.ParameterType.GetGenericArguments().ToList();
                if (genericArguments.Count != 1)
                {
                    continue;
                }

                return method;
            }

            throw new InvalidOperationException("Expected Delete method not found");
        }

        private static IEnumerable<Type> GetInheritedTypes<T>() where T : IData
        {
            var rootType = typeof(T);

            return InheritanceMap.GetOrAdd(rootType, t => ResolveInheritedTypes<T>());
        }

        private static IList<Type> ResolveInheritedTypes<T>() where T : IData
        {
            var rootType = typeof(T);
            if (rootType.GetCustomAttribute<ImmutableTypeIdAttribute>() != null)
            {
                throw new ArgumentException("Provided type is not a valid root for inheritance since it has as Immutable Type Id");
            }

            var list = new List<Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsInterface
                            && rootType.IsAssignableFrom(t)
                            && t.GetCustomAttribute<ImmutableTypeIdAttribute>() != null);

                    list.AddRange(types);
                }
                catch { }
            }

            return list;
        }
    }
}
