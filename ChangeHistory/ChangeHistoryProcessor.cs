using System;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.ChangeHistory
{
    public static class ChangeHistoryProcessor
    {
        public static void Initialize()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (typeof(IChangeHistory).IsAssignableFrom(type) && !typeof(IPage).IsAssignableFrom(type))
                    {
                        DataEventSystemFacade.SubscribeToDataBeforeAdd(type, SetChangeHistory, false);
                        DataEventSystemFacade.SubscribeToDataBeforeUpdate(type, SetChangeHistory, false);
                    }
                }
            }            
        }

        private static void SetChangeHistory(object sender, DataEventArgs e)
        {
            var instance = (IChangeHistory)e.Data;

            instance.ChangeDate = DateTime.Now;
            instance.ChangedBy = UserValidationFacade.GetUsername();

            DataFacade.Update(instance, true, true, true);
        }
    }
}
