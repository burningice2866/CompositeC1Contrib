using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

using Composite;
using Composite.C1Console.Security;
using Composite.Core;
using Composite.Core.Extensions;
using Composite.Core.Xml;
using Composite.Functions;
using Composite.Plugins.Functions.FunctionProviders.FileBasedFunctionProvider;

namespace CompositeC1Contrib.Rendering.Mvc.Functions
{
    public class MvcFunction : IFunction
    {
        private static readonly string LogTitle = typeof(MvcFunction).FullName;

        private readonly Type _modelType;
        private readonly ControllerDescriptor _controllerDescriptor;

        public EntityToken EntityToken
        {
            get { return new MvcFunctionEntityToken(this); }
        }

        public string Namespace { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<ParameterProfile> ParameterProfiles
        {
            get
            {
                var list = new List<ParameterProfile>();

                if (_modelType == null)
                {
                    return list;
                }

                var parammeters = GetModelParameters(_modelType);

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

        public Type ReturnType
        {
            get { return typeof(XhtmlDocument); }
        }

        public MvcFunction(ControllerDescriptor controllerDescriptor)
        {
            Verify.ArgumentNotNull(controllerDescriptor, "controllerDescriptor");

            var attribute = controllerDescriptor.GetCustomAttributes(false).OfType<MvcFunctionAttribute>().SingleOrDefault();
            Verify.ArgumentCondition(attribute != null, "controllerDescriptor", String.Format("Controller '{0} is missing the '{1}' attribute", controllerDescriptor.ControllerType.Name, typeof(MvcFunctionAttribute).Name));

            _controllerDescriptor = controllerDescriptor;

            var c1FunctionsType = typeof(C1FunctionsController);
            var baseType = _controllerDescriptor.ControllerType.BaseType;

            Verify.ArgumentCondition(c1FunctionsType.IsAssignableFrom(baseType), "controllerDescriptor", String.Format("Controller '{0} needs to inherit from '{1}'", controllerDescriptor.ControllerType.Name, c1FunctionsType.Name));

            if (baseType.IsGenericType)
            {
                var typeArguments = baseType.GetGenericArguments();

                _modelType = typeArguments[0];
            }

            Namespace = String.IsNullOrEmpty(attribute.Namespace) ? _controllerDescriptor.ControllerType.Namespace : attribute.Namespace;
            Name = String.IsNullOrEmpty(attribute.Name) ? _controllerDescriptor.ControllerName : attribute.Name;
            Description = attribute.Description ?? String.Empty;
        }

        public object Execute(ParameterList parameters, FunctionContextContainer context)
        {
            var viewContext = (ViewContext)context.GetParameterValue("viewContext", typeof(ViewContext));
            var htmlHelper = (HtmlHelper)viewContext.TempData["HtmlHelper"];

            var routeValues = new Dictionary<string, object>
            {
                {"PageModel", viewContext.ViewData.Model}
            };

            if (_modelType != null)
            {
                var model = Activator.CreateInstance(_modelType);

                foreach (var parameter in ParameterProfiles)
                {
                    var name = parameter.Name;
                    var value = parameters.GetParameter(name);

                    _modelType.GetProperty(name).SetValue(model, value);
                }

                routeValues.Add("FunctionModel", model);
            }

            var functionaParameters = new RouteValueDictionary(routeValues);

            var html = htmlHelper.Action("Index", _controllerDescriptor.ControllerName, functionaParameters);

            return XhtmlDocument.Parse(html.ToString());
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
