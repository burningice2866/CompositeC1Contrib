using System.Web.Mvc;

namespace CompositeC1Contrib.Rendering.Mvc.Functions
{
    public abstract class C1FunctionsController : Controller { }

    public abstract class C1FunctionsController<TFunctionModel> : C1FunctionsController
    {
        public TFunctionModel FunctionModel
        {
            get { return (TFunctionModel)RouteData.Values["FunctionModel"]; }
        }

        protected override PartialViewResult PartialView(string viewName, object model)
        {
            var view = base.PartialView(viewName, model);

            view.ViewData.Add("FunctionModel", FunctionModel);

            return view;
        }
    }

    public abstract class C1FunctionsController<TFunctionModel, TPageModel> : C1FunctionsController<TFunctionModel>
    {
        public TPageModel PageModel
        {
            get { return (TPageModel)RouteData.Values["PageModel"]; }
        }

        protected override PartialViewResult PartialView(string viewName, object model)
        {
            var view = base.PartialView(viewName, model);

            view.ViewData.Add("PageModel", PageModel);

            return view;
        }
    }
}
