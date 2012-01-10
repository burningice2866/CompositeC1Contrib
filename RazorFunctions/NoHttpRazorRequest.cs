using System.Web;

namespace CompositeC1Contrib.RazorFunctions
{
    public class NoHttpRazorRequest : HttpRequestBase
    {
        public override bool IsLocal
        {
            get { return false; }
        }
    }
}
