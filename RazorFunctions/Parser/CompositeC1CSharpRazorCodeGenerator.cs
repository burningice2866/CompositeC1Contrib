using System.CodeDom;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser.SyntaxTree;

namespace CompositeC1Contrib.RazorFunctions.Parser
{
    public class CompositeC1CSharpRazorCodeGenerator : CSharpRazorCodeGenerator
    {
        public bool StrictMode { get; private set; }

        public CompositeC1CSharpRazorCodeGenerator(string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host, bool strictMode)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
            StrictMode = strictMode;
        }

        protected override bool TryVisitSpecialSpan(Span span)
        {
            return TryVisit<ReturnTypeSpan>(span, VisitReturnTypeSpan);
        }

        public override void VisitError(RazorError err)
        {
            if (StrictMode)
            {
                throw new TemplateParsingException(err);
            }

            base.VisitError(err);
        }

        private void VisitReturnTypeSpan(ReturnTypeSpan span)
        {
            if (DesignTimeMode)
            {
                WriteHelperVariable(span.ReturnType, "__returnTypeHelper");
            }

            var attributeType = new CodeTypeReference(typeof(FunctionReturnTypeAttribute));
            var attributeArgument = new CodeAttributeArgument("ReturnType", new CodeTypeOfExpression(span.ReturnType));
            
            var attr = new CodeAttributeDeclaration(attributeType, new [] { attributeArgument });

            GeneratedClass.CustomAttributes.Add(attr);
        }
    }
}
