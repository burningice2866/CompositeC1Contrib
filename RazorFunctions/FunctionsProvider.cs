using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;

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
                var path = HostingEnvironment.MapPath("~/App_Data/Razor");
                DirectoryInfo dirInfo = new DirectoryInfo(path);

                var files = dirInfo.EnumerateFiles("*.cshtml", SearchOption.AllDirectories);

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

                    yield return new RazorFunction(ns, name);
                }
            }
        }
    }
}
