namespace CompositeC1Contrib.Rendering.Mvc.Functions
{
    public abstract class C1FunctionsController<T> : C1FunctionsController
    {
        public T FunctionModel
        {
            get { return (T)RouteData.Values["FunctionModel"]; }
        }
    }
}
