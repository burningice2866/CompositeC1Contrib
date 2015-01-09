using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

using Composite;
using Composite.C1Console.Security;
using Composite.Core;
using Composite.Core.Extensions;
using Composite.Core.Routing.Pages;
using Composite.Core.Xml;
using Composite.Functions;
using Composite.Plugins.Functions.FunctionProviders.FileBasedFunctionProvider;

using Nancy;

namespace CompositeC1Contrib.Rendering.Nancy.Functions
{
    public class NancyFunction : IFunction
    {
        private static readonly string LogTitle = typeof(NancyFunction).FullName;

        private readonly Type _moduleType;

        public string Namespace { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public EntityToken EntityToken
        {
            get { return new NancyFunctionEntityToken(this); }
        }

        public Type ReturnType
        {
            get { return typeof(XhtmlDocument); }
        }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                var list = new List<ParameterProfile>();

                var parammeters = GetModelParameters(_moduleType);

                foreach (var parameter in parammeters.Values)
                {
                    BaseValueProvider fallbackValueProvider = new NoValueValueProvider();
                    WidgetFunctionProvider widgetFunctionProvider = null;

                    var name = parameter.Name;
                    var required = true;
                    var helpText = String.Empty;
                    var hideInSimpleView = false;

                    if (parameter.Attribute != null)
                    {
                        if (!parameter.Attribute.Label.IsNullOrEmpty())
                        {
                            name = parameter.Attribute.Label;
                        }

                        if (!parameter.Attribute.Help.IsNullOrEmpty())
                        {
                            helpText = parameter.Attribute.Help;
                        }

                        required = !parameter.Attribute.HasDefaultValue;
                        if (!required)
                        {
                            fallbackValueProvider = new ConstantValueProvider(parameter.Attribute.DefaultValue);
                        }

                        widgetFunctionProvider = parameter.WidgetProvider;
                        hideInSimpleView = parameter.Attribute.HideInSimpleView;
                    }

                    if (widgetFunctionProvider == null)
                    {
                        widgetFunctionProvider = StandardWidgetFunctions.GetDefaultWidgetFunctionProviderByType(parameter.Type, required);
                    }

                    list.Add(new ParameterProfile(
                        parameter.Name,
                        parameter.Type,
                        required,
                        fallbackValueProvider,
                        widgetFunctionProvider,
                        name,
                        new HelpDefinition(helpText), hideInSimpleView));
                }

                return list;
            }
        }

        public NancyFunction(Type moduleType)
        {
            _moduleType = moduleType;

            var attribute = _moduleType.GetCustomAttribute<NancyFunctionAttribute>();

            Verify.ArgumentCondition(attribute != null, "moduleType", String.Format("Module '{0} is missing the '{1}' attribute", _moduleType.Name, typeof(NancyFunctionAttribute).Name));

            Namespace = String.IsNullOrEmpty(attribute.Namespace) ? _moduleType.Namespace : attribute.Namespace;
            Name = String.IsNullOrEmpty(attribute.Name) ? _moduleType.Name : attribute.Name;
            Description = attribute.Description ?? String.Empty;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var pathInfo = C1PageRoute.GetPathInfo();
            var path = "/" + Namespace + "." + Name + pathInfo;

            var response = new NancyC1FunctionHost().ProcessRequest(httpContext, path, parameters);
            
            try
            {
                var document = ParseContent(response.Content);

                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    C1PageRoute.RegisterPathInfoUsage();
                }

                return document;
            }
            catch (Exception)
            {
                if (response.Content.Contains("<title>404</title>"))
                {
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    const string startTag = "<pre id=\"errorContents\">";
                    const string endTag = "</pre>";

                    var start = response.Content.IndexOf(startTag, StringComparison.OrdinalIgnoreCase) + startTag.Length;
                    var end = response.Content.IndexOf(endTag, start, StringComparison.OrdinalIgnoreCase);

                    var error = response.Content.Substring(start, end - start);

                    C1PageRoute.RegisterPathInfoUsage();

                    throw new HttpException(error);
                }

                httpContext.Response.Clear();
                httpContext.Response.Write(response.Content);
                httpContext.Response.End();

                return null;
            }
        }

        public static XhtmlDocument ParseContent(string content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return new XhtmlDocument();
            }

            if (content.StartsWith("<html"))
            {
                try
                {
                    return XhtmlDocument.Parse(content);
                }
                catch (Exception) { }
            }

            return XhtmlDocument.Parse("<html xmlns='{0}'><head/><body>{1}</body></html>".FormatWith(new object[] { Namespaces.Xhtml, content }));
        }

        private static IDictionary<string, FunctionParameter> GetModelParameters(Type modelType)
        {
            var dictionary = new Dictionary<string, FunctionParameter>();

            foreach (var info in modelType.GetProperties(BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (((info.GetSetMethod(false) == null)) ||
                    info.GetCustomAttributes(typeof(FunctionParameterIgnoreAttribute), false).Any())
                {
                    continue;
                }

                var propertyType = info.PropertyType;
                var name = info.Name;

                FunctionParameterAttribute functionParameterAttribute = null;

                var source = info.GetCustomAttributes<FunctionParameterAttribute>(false).ToList();
                if (source.Count > 1)
                {
                    Log.LogWarning(LogTitle, String.Format("More than one '{0}' attribute defined on property '{1}'", typeof(FunctionParameterAttribute).Name, name));
                }
                else
                {
                    functionParameterAttribute = source.FirstOrDefault();
                }

                WidgetFunctionProvider widgetProvider = null;
                if ((functionParameterAttribute != null) && functionParameterAttribute.HasWidgetMarkup)
                {
                    try
                    {
                        widgetProvider = functionParameterAttribute.GetWidgetFunctionProvider(modelType, info);
                    }
                    catch (Exception exception)
                    {
                        Log.LogWarning(LogTitle, String.Format("Failed to get widget function provider for parameter property {0}.", info.Name));
                        Log.LogWarning(LogTitle, exception);
                    }
                }

                if (!dictionary.ContainsKey(name))
                {
                    dictionary.Add(name, new FunctionParameter(name, propertyType, functionParameterAttribute, widgetProvider));
                }
            }

            return dictionary;
        }
    }
}
