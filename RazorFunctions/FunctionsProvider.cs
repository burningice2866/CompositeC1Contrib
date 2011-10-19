using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.WebPages;

using Composite.Functions;
using Composite.Functions.Plugins.FunctionProvider;

namespace CompositeC1Contrib.RazorFunctions
{
    public class FunctionsProvider : IFunctionProvider
    {
        public FunctionNotifier FunctionNotifier
        {
            set { }
        }

        public IEnumerable<IFunction> Functions
        {
            get
            {
                var virtualPath = "~/App_Data/Razor";
                var absolutePath = HostingEnvironment.MapPath(virtualPath);

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

                    ParameterInfo[] parameters = null;
                    var relativeFilePath = Path.Combine(virtualPath, ns.Replace(".", Path.DirectorySeparatorChar.ToString()), name + ".cshtml");

                    var webPage = WebPage.CreateInstanceFromVirtualPath(relativeFilePath);

                    var mainMethod = webPage.GetType().GetMembers().SingleOrDefault(m => m.Name == "main") as MethodInfo;
                    if (mainMethod != null)
                    {
                        parameters = mainMethod.GetParameters();
                    }

                    yield return new RazorFunction(ns, name, parameters, relativeFilePath);
                }
            }
        }
    }
}
