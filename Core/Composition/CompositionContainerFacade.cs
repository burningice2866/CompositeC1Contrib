using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.ComponentModel.Composition.Registration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CompositeC1Contrib.Composition
{
    public static class CompositionContainerFacade
    {
        private static readonly string BinDir;
        private static readonly CompositionContainer GlobalContainer;

        static CompositionContainerFacade()
        {
            var appDomain = AppDomain.CurrentDomain;

            BinDir = Path.Combine(appDomain.BaseDirectory, appDomain.RelativeSearchPath ?? String.Empty);
            GlobalContainer = BuildContainer();
        }

        public static IEnumerable<Type> GetExportedTypes<T>()
        {
            return GetExportedTypes<T>(null);
        }

        public static IEnumerable<Type> GetExportedTypes<T>(Action<RegistrationBuilder> action)
        {
            RegistrationBuilder builder = null;

            if (action != null)
            {
                builder = new RegistrationBuilder();

                action(builder);
            }

            var catalog = new SafeDirectoryCatalog(BinDir, builder);

            return catalog.Parts
                .Select(part => ComposablePartExportType<T>(part))
                .Where(t => t != null);
        }

        public static IEnumerable<T> GetExportedValues<T>()
        {
            return GetExportedValues<T>(GlobalContainer);
        }

        public static IEnumerable<T> GetExportedValues<T>(string contract)
        {
            return GetExportedValues<T>(GlobalContainer, contract);
        }

        public static IEnumerable<T> GetExportedValues<T>(Action<RegistrationBuilder> action)
        {
            return GetExportedValues<T>(action, String.Empty);
        }

        public static IEnumerable<T> GetExportedValues<T>(Action<RegistrationBuilder> action, string contract)
        {
            var builder = new RegistrationBuilder();

            action(builder);

            var container = BuildContainer(builder);

            return GetExportedValues<T>(container, contract);
        }

        private static IEnumerable<T> GetExportedValues<T>(ExportProvider container)
        {
            return GetExportedValues<T>(container, null);
        }

        private static IEnumerable<T> GetExportedValues<T>(ExportProvider container, string contract)
        {
            return String.IsNullOrEmpty(contract) ? container.GetExportedValues<T>() : container.GetExportedValues<T>(contract);
        }

        private static CompositionContainer BuildContainer(ReflectionContext builder = null)
        {
            var batch = new CompositionBatch();
            var catalog = new SafeDirectoryCatalog(BinDir, builder);
            var container = new CompositionContainer(catalog);

            container.Compose(batch);

            return container;
        }

        private static Type ComposablePartExportType<T>(ComposablePartDefinition part)
        {
            return part.ExportDefinitions.Any(DefintionMatch<T>) ? ReflectionModelServices.GetPartType(part).Value : null;
        }

        private static bool DefintionMatch<T>(ExportDefinition def)
        {
            return def.Metadata.ContainsKey("ExportTypeIdentity") && def.Metadata["ExportTypeIdentity"].Equals(typeof(T).FullName);
        }
    }
}
