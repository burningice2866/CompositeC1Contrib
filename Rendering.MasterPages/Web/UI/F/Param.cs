using System.Web.UI;

namespace CompositeC1Contrib.Web.UI.F
{
    [ControlBuilder(typeof(ParamControlBuilder))]
    public class Param : Control, IParserAccessor
    {
        public string Name { get; set; }
        public FunctionParameterValue Value { get; private set; }

        public Param() { }

        public Param(string name, object value)
        {
            Name = name;
            Value = new FunctionParameterValue(value);
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (this.HasControls())
            {
                base.AddParsedSubObject(obj);
            }
            else if (obj is LiteralControl)
            {
                this.Value = new FunctionParameterValue(((LiteralControl)obj).Text);
            }
            else
            {
                base.AddParsedSubObject(obj);
            }
        }

        protected override void DataBindChildren()
        {
            base.DataBindChildren();

            if (Value == null)
            {
                if (Controls.Count > 0)
                {
                    var child = Controls[0];
                    if (child is DataBoundLiteralControl)
                    {
                        Value = new FunctionParameterValue(((DataBoundLiteralControl)child).Text);
                    }
                }
            }
        }
    }
}
