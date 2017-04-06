using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Composite.Data;

namespace CompositeC1Contrib.DataTypesSynchronization
{
    public class DataTypesSynchronizationConfiguration
    {
        public static DataTypesSynchronizationConfiguration Current { get; private set; }

        public IList<IDataProvider> DataProviders { get; private set; }
        public IDictionary<Type, SynchronizationTypeDefinition> TypesToSynchronize { get; private set; }

        public DataTypesSynchronizationConfiguration()
        {
            DataProviders = new List<IDataProvider>();
            TypesToSynchronize = new Dictionary<Type, SynchronizationTypeDefinition>();

            Init();

            Current = this;
        }

        private void Init()
        {
            var types = new List<Type>();

            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var synchronizeTypes = from t in asm.GetTypes()
                                               where t.IsInterface
                                                    && typeof(IData).IsAssignableFrom(t)
                                                    && t.GetCustomAttribute<SynchronizeDataAttribute>() != null
                                               select t;

                        types.AddRange(synchronizeTypes);
                    }
                    catch { }
                }
            }
            catch { }

            var updateModelTypeMethod = typeof(SynchronizationJob).GetMethod("UpdateModelType", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var t in types)
            {
                if (TypesToSynchronize.ContainsKey(t))
                {
                    continue;
                }

                var attribute = (SynchronizeDataAttribute)t.GetCustomAttribute(typeof(SynchronizeDataAttribute));
                var mapperType = attribute.Mapper;

                var modelType = mapperType.GetInterfaces().First().GetGenericArguments().First();

                var mapper = Activator.CreateInstance(mapperType);
                var postProcessor = attribute.PostProcessor == null ? null : Activator.CreateInstance(attribute.PostProcessor);

                var generic = updateModelTypeMethod.MakeGenericMethod(modelType, t);

                TypesToSynchronize.Add(t, new SynchronizationTypeDefinition
                {
                    Type = t,
                    Frequency = attribute.Frequency,
                    Method = generic,
                    Mapper = mapper,
                    PostProcessor = postProcessor
                });
            }
        }
    }
}
