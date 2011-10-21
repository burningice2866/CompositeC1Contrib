using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Web.Razor;
using System.Web.WebPages.Razor;

namespace CompositeC1Contrib.RazorFunctions
{
    public class CompositeC1RazorHost : WebPageRazorHost
    {
        public CompositeC1RazorHost(string virtualPath)
            : base(virtualPath)
        {
        }

        public CompositeC1RazorHost(string virtualPath, string physicalPath)
            : base(virtualPath, physicalPath)
        {
        }

        protected override RazorCodeLanguage GetCodeLanguage()
        {
            if (String.Equals(Path.GetExtension(base.VirtualPath), ".razor", StringComparison.OrdinalIgnoreCase))
            {
                return new CSharpRazorCodeLanguage();
            }

            return base.GetCodeLanguage();
        }

        public override void PostProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeNamespace generatedNamespace, CodeTypeDeclaration generatedClass, CodeMemberMethod executeMethod)
        {
            base.PostProcessGeneratedCode(codeCompileUnit, generatedNamespace, generatedClass, executeMethod);
        }
    }
}
