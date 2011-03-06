using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;

using Composite.C1Console.Security;
using Composite.Data;
using Composite.Data.Types;

namespace CompositeC1Contrib.Web.Mvc
{
    public class ContentController : Controller
    {
        public IPage Document
        {
            get { return (IPage)RouteData.DataTokens["ID"]; }
        }

        public virtual ActionResult Index()
        {
            var model = new PageModel(Document);
            return View(model);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            InitializeCulture(filterContext);
            ValidateViewUnpublishedRequest(filterContext);

            base.OnActionExecuting(filterContext);
        }

        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            var dataScope = RouteData.Values["dataScope"] as DataScope;
            if (dataScope != null)
            {
                dataScope.Dispose();
            }

            base.OnResultExecuted(filterContext);
        }

        protected virtual void ValidateViewUnpublishedRequest(ActionExecutingContext filterContext)
        {
            if (Request.QueryString["dataScope"] == "administrated" && !UserValidationFacade.IsLoggedIn())
            {
                string url = String.Format("{0}/Composite/Login.aspx?ReturnUrl={1}", Composite.Core.WebClient.UrlUtils.PublicRootPath, HttpUtility.UrlEncodeUnicode(Request.Url.OriginalString));
                filterContext.Result = new RedirectResult(url);
            }
        }

        protected virtual void InitializeCulture(ActionExecutingContext filterContext)
        {
            if (ControllerContext.IsChildAction || Document == null)
            {
                return;
            }

            var ci = new CultureInfo(Document.CultureName);
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}
