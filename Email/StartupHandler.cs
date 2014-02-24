using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Composite.Core.Application;
using Composite.Data;
using Composite.Data.DynamicTypes;

using CompositeC1Contrib.Email.Data;
using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email
{
    [ApplicationStartup]
    public sealed class StartupHandler
    {
        public static void OnBeforeInitialize() { }

        public static void OnInitialized()
        {
            DynamicTypeManager.EnsureCreateStore(typeof(IMailQueue));
            DynamicTypeManager.EnsureCreateStore(typeof(IQueuedMailMessage));
            DynamicTypeManager.EnsureCreateStore(typeof(ISentMailMessage));

            using (var data = new DataConnection())
            {
                var mailTemplates = data.Get<IMailTemplate>().ToDictionary(t => t.Key);

                var asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asms)
                {
                    try
                    {
                        AddTemplatesFromProviders(data, mailTemplates, asm);
                        AddTemplatesFromModels(data, mailTemplates, asm);
                    }
                    catch { }
                }
            }

            MailWorker.Initialize();
        }

        private static void AddTemplatesFromProviders(DataConnection data, IDictionary<string, IMailTemplate> mailTemplates, Assembly asm)
        {
            foreach (var type in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IMailTemplatesProvider).IsAssignableFrom(t)))
            {
                var provicer = (IMailTemplatesProvider)Activator.CreateInstance(type);
                var templates = provicer.GetTemplates();

                foreach (var template in templates)
                {
                    EnsureUniqueTemplateKey(mailTemplates, template.Item1);

                    var instance = data.CreateNew<IMailTemplate>();

                    instance.Id = Guid.NewGuid();
                    instance.Key = template.Item1;
                    instance.ModelType = template.Item2.AssemblyQualifiedName;

                    data.Add(instance);
                }
            }
        }

        private static void AddTemplatesFromModels(DataConnection data, IDictionary<string, IMailTemplate> mailTemplates, Assembly asm)
        {
            foreach (var type in asm.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
            {
                var attribute = type.GetCustomAttributes(typeof(MailModelAttribute), false).Cast<MailModelAttribute>().FirstOrDefault();
                if (attribute == null)
                {
                    continue;
                }

                EnsureUniqueTemplateKey(mailTemplates, attribute.Key);

                var template = data.CreateNew<IMailTemplate>();

                template.Id = Guid.NewGuid();
                template.Key = attribute.Key;
                template.ModelType = type.AssemblyQualifiedName;

                data.Add(template);
            }
        }

        private static void EnsureUniqueTemplateKey(IDictionary<string, IMailTemplate> mailTemplates, string key)
        {
            if (mailTemplates.ContainsKey(key))
            {
                throw new InvalidOperationException(String.Format("A template with the key '{0}' already exists", key));
            }
        }
    }
}
