using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml.Linq;

using Composite.Core.Types;
using Composite.Core.WebClient;
using Composite.Core.WebClient.Renderings.Data;
using Composite.Core.WebClient.Renderings.Page;
using Composite.Core.Xml;
using Composite.Data;
using Composite.Data.DynamicTypes;
using Composite.Functions;

namespace CompositeC1Contrib.Email
{
    public class TemplateRenderer
    {
        public static string ResolveText(string text, object mailModel)
        {
            foreach (var property in mailModel.GetType().GetProperties().Where(d => d.CanRead))
            {
                var value = (property.GetValue(mailModel, null) ?? String.Empty).ToString();

                text = text.Replace(String.Format("%{0}%", property.Name), value);
            }

            return text;
        }

        public static string ResolveHtml(string body, object mailModel)
        {
            XhtmlDocument templateMarkup;

            try
            {
                templateMarkup = XhtmlDocument.Parse(body);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Failed to parse markup for file '{0}'", body), ex);
            }

            PageRenderer.ExecuteEmbeddedFunctions(templateMarkup.Root, new FunctionContextContainer());

            ResolveFields(templateMarkup, mailModel);
            templateMarkup = XhtmlDocument.Parse(PageUrlHelper.ChangeRenderingPageUrlsToPublic(templateMarkup.ToString()));
            ResolveRelativeToAbsolutePaths(templateMarkup);

            foreach (var property in mailModel.GetType().GetProperties().Where(d => d.CanRead))
            {
                var aElements = templateMarkup.Descendants().Where(el => el.Name.LocalName == "a").ToList();
                foreach (var a in aElements)
                {
                    var href = a.Attribute("href");
                    if (href != null)
                    {
                        var value = (property.GetValue(mailModel, null) ?? String.Empty).ToString();

                        href.Value = href.Value.Replace(String.Format("%{0}%", property.Name), value);
                    }
                }
            }

            return templateMarkup.ToString();
        }

        private static void ResolveFields(XDocument document, object mailModel)
        {
            var type = mailModel.GetType();
            IDictionary<string, object> objectValues;
            var propertyInfoLookup = new Dictionary<string, PropertyInfo>();
            var fieldsWithReferenceRendering = new List<string>();

            var data = mailModel as IData;
            if (data != null)
            {
                propertyInfoLookup = type.GetProperties().Where(p => typeof(IData).IsAssignableFrom(p.DeclaringType)).ToList().ToDictionary(p => p.Name);

                foreach (var dataPropertyInfo in propertyInfoLookup.Values)
                {
                    Type referencedType;
                    if (!dataPropertyInfo.TryGetReferenceType(out referencedType))
                    {
                        continue;
                    }

                    bool canRender = DataXhtmlRenderingServices.CanRender(referencedType, XhtmlRenderingType.Embedable);
                    if (canRender)
                    {
                        fieldsWithReferenceRendering.Add(dataPropertyInfo.Name);
                    }
                }

                objectValues = propertyInfoLookup.ToDictionary(f => f.Key, f => f.Value.GetValue(data, new object[] { }));
            }
            else
            {
                objectValues = type.GetProperties().ToDictionary(p => p.Name, p => p.PropertyType == typeof(XElement) ? p.GetValue(mailModel, null) : (object)(p.GetValue(mailModel, null)).ToString());
            }

            var fieldReferenceElements = document.Descendants(Namespaces.DynamicData10 + "fieldreference");
            foreach (var el in fieldReferenceElements)
            {
                var attr = el.Attribute("typemanagername");
                var t = TypeManager.GetType(attr.Value);

                if (t != null && t == type)
                {
                    attr.Value = type.AssemblyQualifiedName;
                }
            }

            var references = DynamicTypeMarkupServices.GetFieldReferenceDefinitions(document, type.AssemblyQualifiedName).ToList();
            foreach (var reference in references)
            {
                object value = null;
                try
                {
                    if (fieldsWithReferenceRendering.Contains(reference.FieldName))
                    {
                        // reference field with rendering...
                        Type referencedType;
                        if (propertyInfoLookup[reference.FieldName].TryGetReferenceType(out referencedType))
                        {
                            if (objectValues[reference.FieldName] != null)
                            {
                                var dataReference = DataReferenceFacade.BuildDataReference(referencedType, objectValues[reference.FieldName]);

                                value = DataXhtmlRenderingServices.Render(dataReference, XhtmlRenderingType.Embedable).Root;
                            }
                        }
                    }
                }
                finally
                {
                    if (value == null)
                    {
                        if (objectValues.ContainsKey(reference.FieldName)) // prevents unknown props from creating exceptions
                        {
                            value = objectValues[reference.FieldName];
                        }
                    }

                    reference.FieldReferenceElement.ReplaceWith(value);
                }
            }
        }

        private static void ResolveRelativeToAbsolutePaths(XhtmlDocument xhtmlDocument)
        {
            var ctx = HttpContext.Current;
            if (ctx == null)
            {
                return;
            }

            var xhtmlElements = xhtmlDocument.Descendants().Where(f => f.Name.Namespace == Namespaces.Xhtml);
            var pathAttributes = xhtmlElements.Attributes().Where(f => f.Name.LocalName == "src" || f.Name.LocalName == "href" || f.Name.LocalName == "action");

            var relativePathAttributes = pathAttributes.Where(f => f.Value.StartsWith("/")).ToList();
            if (!relativePathAttributes.Any())
            {
                return;
            }

            var hostName = ctx.Request.Url.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped);
            var baseUri = new Uri(hostName);

            foreach (var relativePathAttribute in relativePathAttributes)
            {
                relativePathAttribute.Value = new Uri(baseUri, relativePathAttribute.Value).OriginalString;
            }
        }
    }
}
