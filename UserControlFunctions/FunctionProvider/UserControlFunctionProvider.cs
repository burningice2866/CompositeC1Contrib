using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web.Hosting;
using System.Web.UI;

using Composite.Core;
using Composite.Core.IO;
using Composite.Core.Threading;
using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

using CompositeC1Contrib.FunctionProvider;

namespace CompositeC1Contrib.UserControlFunctions.FunctionProvider
{
    public class UserControlFunctionProvider : IFunctionProvider
    {
        private static readonly object _lock = new object();

        private static readonly string virtualPath = "~/App_Data/UserControl";
        private static readonly string physicalPath = HostingEnvironment.MapPath(virtualPath);

        private readonly IDictionary<string, UserControlFunction> _functionCache = new Dictionary<string, UserControlFunction>();

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
                var returnList = new List<UserControlFunction>();

                var files = new DirectoryInfo(physicalPath).EnumerateFiles("*.ascx", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var parts = file.FullName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                    string ns = "";
                    string name = Path.GetFileNameWithoutExtension(parts[parts.Length - 1]);

                    for (int i = parts.Length - 2; i > 0; i--)
                    {
                        if (parts[i].Equals("UserControl", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        ns = parts[i] + "." + ns;
                    }

                    ns = ns.Substring(0, ns.Length - 1);

                    var relativeFilePath = Path.Combine(virtualPath, ns.Replace(".", Path.DirectorySeparatorChar.ToString()), name + ".ascx");

                    UserControl control = null;

                    try
                    {
                        var p = new Page();
                        control = (UserControl)p.LoadControl(relativeFilePath);
                    }
                    catch (Exception exc)
                    {
                        Log.LogError("Error in instantiating UserControl function", exc);

                        if (_functionCache.ContainsKey(relativeFilePath))
                        {
                            returnList.Add(_functionCache[relativeFilePath]);
                        }

                        continue;
                    }

                    var parameters = getParameters(control).ToDictionary(p => p.Name);
                    var description = getDescription(control);

                    var razorFunction = new UserControlFunction(ns, name, description, parameters, relativeFilePath);

                    _functionCache[relativeFilePath] = razorFunction;

                    returnList.Add(razorFunction);
                    
                }

                return returnList;
            }
        }

        public UserControlFunctionProvider()
        {
            var watcher = new C1FileSystemWatcher(physicalPath, "*.ascx.cs")
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

        private string getDescription(UserControl control)
        {
            var attr = control.GetType().GetCustomAttributes(typeof(FunctionDescriptionAttribute), false).Cast<FunctionDescriptionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Description;
            }

            return "A UserControl function";
        }

        private IEnumerable<FunctionParameterHolder> getParameters(UserControl control)
        {
            var type = control.GetType();
            while (type != typeof(UserControl))
            {
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.DeclaredOnly);
                foreach (var prop in properties)
                {
                    var propType = prop.PropertyType;
                    var att = prop.GetCustomAttributes(typeof(FunctionParameterAttribute), false).Cast<FunctionParameterAttribute>().FirstOrDefault();

                    yield return new FunctionParameterHolder(prop.Name, propType, att);
                }

                type = type.BaseType;
            }            
        }
    }
}
