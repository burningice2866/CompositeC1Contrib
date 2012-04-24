using System.ComponentModel;
using System.Web.UI;

namespace CompositeC1Contrib.Web.UI.F
{
    [ControlBuilder(typeof(ParamControlBuilder))]
    public class Param : Control, IParserAccessor
    {
        private object _value;

        [TypeConverter(typeof(StringToObjectConverter))]
        public object Value
        {
            get { return _value; }

            set
            {
                if (value is ParamObjectConverter)
                {
                    _value = ((ParamObjectConverter)value).Value;
                }
                else
                {
                    _value = value;
                }
            }

        }

        public string Name { get; set; }



        public Param() { }

        public Param(string name, object value)
        {
            Name = name;
            Value = value;
        }

        protected override void AddParsedSubObject(object obj)
        {
            if (this.HasControls())
            {
                base.AddParsedSubObject(obj);
            }
            else if (obj is LiteralControl)
            {
                this.Value = ((LiteralControl)obj).Text;
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
                        Value = ((DataBoundLiteralControl)child).Text;
                    }
                }
            }
        }
    }
}
