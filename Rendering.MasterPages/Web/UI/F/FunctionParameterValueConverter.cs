using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace CompositeC1Contrib.Web.UI.F
{
    public class FunctionParameterValueConverter : TypeConverter 
    {
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return true;
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            return new FunctionParameterValueConverter();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return new FunctionParameterValue(value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                var ci = typeof(FunctionParameterValue).GetConstructor(new[] { typeof(object) });

                return new InstanceDescriptor(ci, new[] { ((FunctionParameterValue)value).Value });
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
