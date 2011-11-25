using System.CodeDom;
using System.Web.UI;

namespace CompositeC1Contrib.Web.UI.F
{
    public class ParamControlBuilder : ControlBuilder
    {
        public ParamControlBuilder()
        {
        }

        public override bool HasAspCode
        {
            get
            {
                return base.HasAspCode;
            }
        }

        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            base.AppendSubBuilder(subBuilder);
        }

        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
            base.ProcessGeneratedCode(codeCompileUnit, baseType, derivedType, buildMethod, dataBindingMethod);
        }

        public override bool AllowWhitespaceLiterals()
        {
            return false;
        }

        public override bool HtmlDecodeLiterals()
        {
            return true;
        }
    }
}
