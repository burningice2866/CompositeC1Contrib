using System;

namespace CompositeC1Contrib.RazorFunctions.Parser
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FunctionReturnTypeAttribute : Attribute
    {
        public Type ReturnType { get; set; }

        public FunctionReturnTypeAttribute()
        {            
        }

        public FunctionReturnTypeAttribute(Type returnType)
        {
            ReturnType = returnType;
        }
    }
}
