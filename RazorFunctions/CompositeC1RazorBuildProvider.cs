using System.CodeDom.Compiler;
using System.Collections;
using System.Web.Compilation;
using System.Web.WebPages.Razor;

namespace CompositeC1Contrib.RazorFunctions
{
    public class CompositeC1RazorBuildProvider : RazorBuildProvider
    {
        public CompositeC1RazorBuildProvider()
        {
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            base.GenerateCode(assemblyBuilder);
        }

        protected override System.CodeDom.CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            return base.GetCodeCompileUnit(out linePragmasTable);
        }

        public override string GetCustomString(CompilerResults results)
        {
            return base.GetCustomString(results);
        }
    }
}
