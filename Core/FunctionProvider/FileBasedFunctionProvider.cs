using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;

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

        private DateTime _lastUpdateTime;

        protected abstract string FileExtension { get; }
        protected abstract string Folder { get; }
        protected abstract Type BaseType { get; }

        public FunctionNotifier FunctionNotifier { private get; set; }
        public string VirtualPath { get; private set; }
        public string PhysicalPath { get; private set; }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var returnList = new List<FileBasedFunction<T>>();

                var files = new DirectoryInfo(PhysicalPath).EnumerateFiles("*." + FileExtension, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var parts = file.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                    string ns = String.Empty;
                    string name = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

                    for (int i = parts.Length - 2; i > 0; i--)
                    {
                        if (parts[i].Equals(Folder, StringComparison.OrdinalIgnoreCase))
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
                        Log.LogError(String.Format("Error instantiating {0} function", Folder), exc);

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

        public FileBasedFunctionProvider()
        {
            VirtualPath = "~/App_Data/" + Folder;
            PhysicalPath = HostingEnvironment.MapPath(VirtualPath);

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

        private IDictionary<string, FunctionParameterHolder> getParameters(object obj)
        {
            var dict = new Dictionary<string, FunctionParameterHolder>();

            var type = obj.GetType();
            while (type != BaseType)
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
                foreach (var prop in properties)
                {
                    var propType = prop.PropertyType;
                    var att = prop.GetCustomAttributes(typeof(FunctionParameterAttribute), false).Cast<FunctionParameterAttribute>().FirstOrDefault();
                    var name = prop.Name;

                    if (!dict.ContainsKey(name))
                    {
                        dict.Add(name, new FunctionParameterHolder(name, propType, att));
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

            return String.Format("A {0} function", Folder);
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
