using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using System.Web.WebPages;

using Composite.Core;
using Composite.Core.IO;
using Composite.Core.Threading;
using Composite.Core.Xml;
using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

using CompositeC1Contrib.FunctionProvider;
using CompositeC1Contrib.RazorFunctions.Parser;

namespace CompositeC1Contrib.RazorFunctions.FunctionProvider
{
    public class RazorFunctionProvider : IFunctionProvider
    {
        private static readonly object _lock = new object();

        private static readonly string virtualPath = "~/App_Data/Razor";
        private static readonly string physicalPath = HostingEnvironment.MapPath(virtualPath);

        private readonly IDictionary<string, RazorFunction> _functionCache = new Dictionary<string, RazorFunction>();

        private DateTime _lastUpdateTime;

        private FunctionNotifier _globalNotifier;
        public FunctionNotifier FunctionNotifier
        {
            set { _globalNotifier = value; }
        }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var returnList = new List<RazorFunction>();

                var files = new DirectoryInfo(physicalPath).EnumerateFiles("*.cshtml", SearchOption.AllDirectories);
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

                        if (_functionCache.ContainsKey(relativeFilePath))
                        {
                            returnList.Add(_functionCache[relativeFilePath]);
                        }

                        continue;
                    }

                    var parameters = getParameters(webPage).ToDictionary(p => p.Name);
                    var returnType = getReturnType(webPage);
                    var description = getDescription(webPage);

                    var razorFunction = new RazorFunction(ns, name, description, parameters, returnType, relativeFilePath);

                    _functionCache[relativeFilePath] = razorFunction;

                    returnList.Add(razorFunction);
                }

                return returnList;
            }
        }

        public RazorFunctionProvider()
        {
            var watcher = new C1FileSystemWatcher(physicalPath, "*.cshtml")
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
            if (_globalNotifier != null)
            {
                lock (_lock)
                {
                    var timeSpan = DateTime.Now - _lastUpdateTime;
                    if (timeSpan.TotalMilliseconds > 100)
                    {
                        Thread.Sleep(50);

                        using (ThreadDataManager.EnsureInitialize())
                        {
                            _globalNotifier.FunctionsUpdated();
                        }

                        _lastUpdateTime = DateTime.Now;
                    }
                }
            }
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

        private string getDescription(WebPageBase webPage)
        {
            var attr = webPage.GetType().GetCustomAttributes(typeof(FunctionDescriptionAttribute), false).Cast<FunctionDescriptionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Description;
            }

            return "A Razor function";
        }

        private IEnumerable<FunctionParameterHolder> getParameters(WebPageBase webPage)
        {
            var properties = webPage.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
            foreach (var prop in properties)
            {
                var type = prop.PropertyType;
                var att = prop.GetCustomAttributes(typeof(FunctionParameterAttribute), false).Cast<FunctionParameterAttribute>().FirstOrDefault();

                yield return new FunctionParameterHolder(prop.Name, type, att);
            }
        }
    }
}
