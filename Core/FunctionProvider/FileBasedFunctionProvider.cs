using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using System.Xml.Linq;

using Composite.Core;
using Composite.Core.IO;
using Composite.Core.Threading;
using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.FunctionProvider
{
    public abstract class FileBasedFunctionProvider<T> : IFunctionProvider where T : FileBasedFunction<T>
    {
        private static readonly object _lock = new object();

        private readonly IDictionary<string, FileBasedFunction<T>> _functionCache = new Dictionary<string, FileBasedFunction<T>>();

        private C1FileSystemWatcher _watcher;
        private DateTime _lastUpdateTime;
        private string _rootFolder;
        private string _name;

        protected abstract string FileExtension { get; }
        protected abstract Type BaseType { get; }

        public FunctionNotifier FunctionNotifier { private get; set; }
        public string VirtualPath { get; private set; }
        public string PhysicalPath { get; private set; }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var returnList = new List<FileBasedFunction<T>>();

                var files = new DirectoryInfo(PhysicalPath).EnumerateFiles("*." + FileExtension, SearchOption.AllDirectories).Where(f => !f.Name.StartsWith("_", StringComparison.Ordinal));
                foreach (var file in files)
                {
                    var parts = file.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                    string ns = String.Empty;
                    string name = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

                    for (int i = parts.Length - 2; i > 0; i--)
                    {
                        if (parts[i].Equals(_rootFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        ns = parts[i] + "." + ns;
                    }

                    ns = ns.Substring(0, ns.Length - 1);

                    var virtualPath = Path.Combine(VirtualPath, ns.Replace(".", Path.DirectorySeparatorChar.ToString()), name + "." + FileExtension);
                    object obj = null;

                    try
                    {
                        obj = InstantiateFile(virtualPath);
                    }
                    catch (Exception exc)
                    {
                        Log.LogError(String.Format("Error instantiating {0} function", _name), exc);

                        if (_functionCache.ContainsKey(virtualPath))
                        {
                            returnList.Add(_functionCache[virtualPath]);
                        }

                        continue;
                    }

                    var parameters = getParameters(obj);
                    var returnType = GetReturnType(obj);
                    var description = getDescription(obj);

                    var function = (FileBasedFunction<T>)typeof(T).GetConstructors().First().Invoke(new object[] { ns, name, description, parameters, returnType, virtualPath, this });

                    _functionCache[virtualPath] = function;

                    returnList.Add(function);
                }

                return returnList;
            }
        }

        public FileBasedFunctionProvider(string name, string folder)
        {
            _name = name;

            VirtualPath = folder;
            PhysicalPath = HostingEnvironment.MapPath(VirtualPath);

            _rootFolder = PhysicalPath.Split(new[] { Path.DirectorySeparatorChar }).Last();

            _watcher = new C1FileSystemWatcher(PhysicalPath, "*")
            {
                IncludeSubdirectories = true
            };

            _watcher.Created += watcher_Changed;
            _watcher.Deleted += watcher_Changed;
            _watcher.Changed += watcher_Changed;
            _watcher.Renamed += watcher_Changed;

            _watcher.EnableRaisingEvents = true;
        }

        protected abstract Type GetReturnType(object obj);
        protected abstract object InstantiateFile(string virtualPath);
        protected abstract bool HandleChange(string path);

        private IDictionary<string, FunctionParameterHolder> getParameters(object obj)
        {
            var dict = new Dictionary<string, FunctionParameterHolder>();
            ParameterWidgets widgetProviders = null;

            var type = obj.GetType();
            while (type != BaseType)
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
                foreach (var prop in properties)
                {
                    var propType = prop.PropertyType;
                    var name = prop.Name;
                    var att = prop.GetCustomAttributes(typeof(FunctionParameterAttribute), false).Cast<FunctionParameterAttribute>().FirstOrDefault();
                    WidgetFunctionProvider widgetProvider = null;

                    if (att != null && !String.IsNullOrEmpty(att.WidgetMarkup))
                    {
                        var el = XElement.Parse(att.WidgetMarkup);

                        widgetProvider = new WidgetFunctionProvider(el);
                    }
                    else
                    {
                        if (widgetProviders == null)
                        {
                            var widgetProviderMethod = type.GetMethod("GetParameterWidgets");
                            if (widgetProviderMethod != null && widgetProviderMethod.ReturnType == typeof(ParameterWidgets))
                            {
                                widgetProviders = (ParameterWidgets)widgetProviderMethod.Invoke(obj, null);
                            }
                            else
                            {
                                widgetProviders = new ParameterWidgets();
                            }
                        }

                        if (widgetProviders.ContainsKey(prop))
                        {
                            widgetProvider = widgetProviders[prop];
                        }
                    }

                    if (!dict.ContainsKey(name))
                    {
                        dict.Add(name, new FunctionParameterHolder(name, propType, att, widgetProvider));
                    }
                }

                type = type.BaseType;
            }

            return dict;
        }

        private string getDescription(object obj)
        {
            var attr = obj.GetType().GetCustomAttributes(typeof(FunctionDescriptionAttribute), false).Cast<FunctionDescriptionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Description;
            }

            return String.Format("A {0} function", _name);
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (FunctionNotifier != null && HandleChange(e.FullPath))
            {
                lock (_lock)
                {
                    var timeSpan = DateTime.Now - _lastUpdateTime;
                    if (timeSpan.TotalMilliseconds > 100)
                    {
                        Thread.Sleep(50);

                        using (ThreadDataManager.EnsureInitialize())
                        {
                            FunctionNotifier.FunctionsUpdated();
                        }

                        _lastUpdateTime = DateTime.Now;
                    }
                }
            }
        }
    }
}
