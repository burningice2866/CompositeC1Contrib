using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace CompositeC1Contrib.RazorFunctions.Parser
{
    public class CompositeC1CSharpCodeParser : CSharpCodeParser 
    {
        public CompositeC1CSharpCodeParser()
        {
            RazorKeywords.Add("returnType", WrapSimpleBlockParser(BlockType.Directive, ParseReturnTypeStatement));
        }

        public bool ParseReturnTypeStatement(CodeBlockInfo block)
        {
            string returnType = null;
            var currentLocation = CurrentLocation;
            
            var flag = RequireSingleWhiteSpace();

            var acceptedCharacters = flag ? AcceptedCharacters.None : AcceptedCharacters.Any;

            End(MetaCodeSpan.Create(Context, false, acceptedCharacters));

            Context.AcceptWhiteSpace(false);

            if (ParserHelpers.IsIdentifierStart(CurrentCharacter))
            {
                using (Context.StartTemporaryBuffer())
                {
                    Context.AcceptLine(false);

                    returnType = Context.ContentBuffer.ToString();

                    Context.AcceptTemporaryBuffer();
                }

                Context.AcceptNewLine();
            }
            else
            {
                OnError(currentLocation, "ReturnType keyword must be followed by type name");
            }

            if (HaveContent || flag)
            {
                End(ReturnTypeSpan.Create(Context, returnType));
            }

            return false;
        }
    }
}
