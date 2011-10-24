using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.WebPages;

using Composite.Core;
using Composite.Core.Xml;
using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.RazorFunctions
{
    public class RazorFunctionProvider : IFunctionProvider
    {
        private static string virtualPath = "~/App_Data/Razor";
        private static string absolutePath = HostingEnvironment.MapPath(virtualPath);

        private FunctionNotifier _globalNotifier;
        public FunctionNotifier FunctionNotifier
        {
            set { _globalNotifier = value; }
        }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var files = new DirectoryInfo(absolutePath).EnumerateFiles("*.cshtml", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var parts = file.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                    string ns = "";
                    string name = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

                    for (int i = parts.Length - 2; i > 0; i--)
                    {
                        if (parts[i].Equals("Razor", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        ns = parts[i] + "." + ns;
                    }

                    ns = ns.Substring(0, ns.Length - 1);

                    var relativeFilePath = Path.Combine(virtualPath, ns.Replace(".", Path.DirectorySeparatorChar.ToString()), name + ".cshtml");
                    WebPageBase webPage = null;

                    try
                    {
                        webPage = WebPage.CreateInstanceFromVirtualPath(relativeFilePath);
                    }
                    catch (Exception exc)
                    {
                        Log.LogError("Error in instantiating razor function", exc);

                        continue;
                    }

                    var parameters = getParameters(webPage);
                    var returnType = getReturnType(webPage);

                    yield return new RazorFunction(ns, name, parameters, returnType, relativeFilePath);
                }
            }
        }

        public RazorFunctionProvider()
        {
            var watcher = new FileSystemWatcher(absolutePath, "*.cshtml")
            {
                IncludeSubdirectories = true
            };

            watcher.Created += watcher_Changed;
            watcher.Deleted += watcher_Changed;
            watcher.Changed += watcher_Changed;
            watcher.Renamed += watcher_Changed;

            watcher.EnableRaisingEvents = true;
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            _globalNotifier.FunctionsUpdated();
        }

        private Type getReturnType(WebPageBase webPage)
        {
            var attr = webPage.GetType().GetCustomAttributes(typeof(FunctionReturnTypeAttribute), false).Cast<FunctionReturnTypeAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.ReturnType;
            }

            return typeof(XhtmlDocument);
        }

        private IEnumerable<FunctionParameterHolder> getParameters(WebPageBase webPage)
        {
            var properties = webPage.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                var myProp = prop;
                var type = myProp.PropertyType;
                Action<WebPageBase, object> setValue = (p, o) => myProp.SetValue(p, o, null);

                var att = prop.GetCustomAttributes(typeof(FunctionParameterAttribute), false).Cast<FunctionParameterAttribute>().FirstOrDefault();

                yield return new FunctionParameterHolder(prop.Name, type, setValue, att);
            }
        }
    }
}
