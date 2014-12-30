using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Composite;
using Composite.Core.PageTemplates;

namespace CompositeC1Contrib.Rendering.Mvc.Templates
{
    public class MvcPageTemplateDescriptor : PageTemplateDescriptor
    {
        private readonly string _defaultPlaceholderId;
        private readonly IEnumerable<PlaceholderDescriptor> _placeholderDescriptions;

        public Tuple<Type, Dictionary<string, PropertyInfo>> TypeInfo { get; private set; }
        public string ViewName { get; private set; }
        public override string DefaultPlaceholderId { get { return _defaultPlaceholderId; } }
        public override IEnumerable<PlaceholderDescriptor> PlaceholderDescriptions { get { return _placeholderDescriptions; } }

        public MvcPageTemplateDescriptor(Type type)
        {
            var attribute = type.GetCustomAttribute<MvcTemplateAttribute>(false);

            Verify.ArgumentCondition(attribute != null, "type", String.Format("Type '{0}' doesn't have the required '{1}' attribute", type.FullName, typeof(MvcTemplateAttribute).FullName));

            var title = attribute.Title;
            if (String.IsNullOrEmpty(title))
            {
                title = type.Name;
            }

            var viewName = attribute.ViewName;
            if (String.IsNullOrEmpty(viewName))
            {
                viewName = title;
            }

            Dictionary<string, PropertyInfo> typeInfo;
            _placeholderDescriptions = ResolvePlaceholderDescriptors(type, out _defaultPlaceholderId, out typeInfo);

            TypeInfo = new Tuple<Type, Dictionary<string, PropertyInfo>>(type, typeInfo);
            ViewName = viewName;
            Id = Guid.Parse(attribute.Id);
            Title = title;
        }

        private static IEnumerable<PlaceholderDescriptor> ResolvePlaceholderDescriptors(Type type, out string defaultId, out Dictionary<string, PropertyInfo> typeInfo)
        {
            defaultId = null;
            var descriptors = new List<PlaceholderDescriptor>();
            var placeholderProperties = new Dictionary<string, PropertyInfo>();

            foreach (var info in type.GetProperties())
            {
                if (info.ReflectedType != info.DeclaringType)
                {
                    continue;
                }

                var customAttributes = info.GetCustomAttributes(typeof(PlaceholderAttribute), true);
                if (customAttributes.Length == 0)
                {
                    continue;
                }

                Verify.That(customAttributes.Length == 1, String.Format("Multiple '{0}' attributes defined on property '{1}'", typeof(PlaceholderAttribute), info.Name));

                var attribute = (PlaceholderAttribute)customAttributes[0];
                var id = attribute.Id ?? info.Name;
                var title = attribute.Title ?? info.Name;

                if (placeholderProperties.ContainsKey(id))
                {
                    throw new InvalidOperationException(String.Format("Placeholder '{0}' defined multiple times", id));
                }

                placeholderProperties.Add(id, info);
                var item = new PlaceholderDescriptor
                {
                    Id = id,
                    Title = title
                };

                descriptors.Add(item);
                if (!attribute.IsDefault)
                {
                    continue;
                }

                Verify.IsNull(defaultId, "More than one placeholder is marked as default");

                defaultId = id;
            }

            if (defaultId == null)
            {
                defaultId = descriptors.First().Id;
            }


            typeInfo = placeholderProperties;

            return descriptors;
        }
    }
}
