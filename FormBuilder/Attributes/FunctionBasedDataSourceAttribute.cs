using Composite.Functions;

namespace CompositeC1Contrib.FormBuilder.Attributes
{
    public class FunctionBasedDataSourceAttribute : DataSourceAttribute
    {
        private string _functioName;

        public FunctionBasedDataSourceAttribute(string functionName)
        {
            _functioName = functionName;
        }

        public override object GetData(BaseForm form)
        {
            var function = FunctionFacade.GetFunction(_functioName);

            return FunctionFacade.Execute<object>(function);
        }
    }
}
