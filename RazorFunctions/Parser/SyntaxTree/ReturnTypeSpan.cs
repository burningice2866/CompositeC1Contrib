using System;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;

namespace CompositeC1Contrib.RazorFunctions.Parser.SyntaxTree
{
    public class ReturnTypeSpan : CodeSpan
    {
        public string ReturnType { get; private set; }

        public ReturnTypeSpan(SourceLocation start, string content)
            : base(start, content)
        {
            this.ReturnType = content.Trim();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (ReturnType ?? String.Empty).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var span = obj as ReturnTypeSpan;

            return span != null && Equals(span);
        }

        private bool Equals(ReturnTypeSpan span)
        {
            return base.Equals(span) && String.Equals(ReturnType, span.ReturnType, StringComparison.Ordinal);
        }

        public new static ReturnTypeSpan Create(ParserContext context, string returnType)
        {
            return new ReturnTypeSpan(context.CurrentSpanStart, returnType);
        }
    }
}
