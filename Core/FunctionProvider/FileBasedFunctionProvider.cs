using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        private readonly object _lock = new object();

        private readonly IDictionary<string, FileBasedFunction<T>> _functionCache = new Dictionary<string, FileBasedFunction<T>>();

        private DateTime _lastUpdateTime;
        private readonly string _rootFolder;
        private readonly string _name;

        protected abstract string FileExtension { get; }
        protected abstract Type BaseType { get; }

        public FunctionNotifier FunctionNotifier { private get; set; }
        public string VirtualPath { get; }
        public string PhysicalPath { get; }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var returnList = new List<FileBasedFunction<T>>();

                var files = new C1DirectoryInfo(PhysicalPath).GetFiles("*." + FileExtension, SearchOption.AllDirectories).Where(f => !f.Name.StartsWith("_", StringComparison.Ordinal));
                foreach (var file in files)
                {
                    var parts = file.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                    var ns = String.Empty;
                    var name = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

                    for (var i = parts.Length - 2; i > 0; i--)
                    {
                        if (parts[i].Equals(_rootFolder, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        ns = parts[i] + "." + ns;
                    }

                    ns = ns.Substring(0, ns.Length - 1);

                    var virtualPath = Path.Combine(VirtualPath, ns.Replace(".", Path.DirectorySeparatorChar.ToString()), name + "." + FileExtension);
                    object obj;

                    try
                    {
                        obj = InstantiateFile(virtualPath);
                    }
                    catch (Exception exc)
                    {
                        Log.LogError($"Error instantiating {_name} function", exc);

                        if (_functionCache.ContainsKey(virtualPath))
                        {
                            returnList.Add(_functionCache[virtualPath]);
                        }

                        continue;
                    }

                    var parameters = GetParameters(obj);
                    var returnType = GetReturnType(obj);
                    var description = GetDescription(obj);

                    var function = (FileBasedFunction<T>)typeof(T).GetConstructors().First().Invoke(new object[] { ns, name, description, parameters, returnType, virtualPath, this });

                    _functionCache[virtualPath] = function;

                    returnList.Add(function);
                }

                return returnList;
            }
        }

        protected FileBasedFunctionProvider(string name, string folder)
        {
            _name = name;

            VirtualPath = folder;
            PhysicalPath = PathUtil.Resolve(VirtualPath);

            _rootFolder = PhysicalPath.Split(Path.DirectorySeparatorChar).Last();

            var watcher = new C1FileSystemWatcher(PhysicalPath, "*")
            {
                IncludeSubdirectories = true
            };

            watcher.Created += watcher_Changed;
            watcher.Deleted += watcher_Changed;
            watcher.Changed += watcher_Changed;
            watcher.Renamed += watcher_Changed;

            watcher.EnableRaisingEvents = true;
        }

        protected abstract Type GetReturnType(object obj);
        protected abstract object InstantiateFile(string virtualPath);
        protected abstract bool HandleChange(string path);

        private IDictionary<string, FunctionParameterHolder> GetParameters(object obj)
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

        private string GetDescription(object obj)
        {
            var attr = obj.GetType().GetCustomAttributes(typeof(FunctionDescriptionAttribute), false).Cast<FunctionDescriptionAttribute>().FirstOrDefault();

            return attr != null ? attr.Description : $"A {_name} function";
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (FunctionNotifier == null || !HandleChange(e.FullPath))
            {
                return;
            }

            lock (_lock)
            {
                var timeSpan = DateTime.Now - _lastUpdateTime;
                if (!(timeSpan.TotalMilliseconds > 100))
                {
                    return;
                }

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
