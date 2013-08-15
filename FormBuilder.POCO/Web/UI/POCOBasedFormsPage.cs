using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

using Composite.AspNet.Razor;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class POCOBasedFormsPage<T> : FormsPage where T : IPOCOForm
    {
        protected T Form { get; private set; }

        public override void ExecutePageHierarchy()
        {
            Form = Activator.CreateInstance<T>();
            RenderingModel = POCOFormTypeHelper.FromBaseForm<T>(Form, Options);

            if (!IsOwnSubmit)
            {
                if (Form is IProvidesDefaultValues)
                {
                    ((IProvidesDefaultValues)Form).SetDefaultValues();

                    foreach (var prop in Form.GetType().GetProperties())
                    {
                        var field = RenderingModel.Fields.SingleOrDefault(f => f.Name == prop.Name);
                        if (field != null && field.ValueType == prop.PropertyType)
                        {
                            field.Value = prop.GetValue(Form, null);
                        }
                    }
                }
            }

            var modelProp = typeof(T).GetProperties().SingleOrDefault(p => p.PropertyType == typeof(FormModel));
            if (modelProp != null)
            {
                modelProp.SetValue(Form, RenderingModel, null);
            }

            base.ExecutePageHierarchy();            
        }

        protected override void OnSubmit()
        {
            foreach (var prop in Form.GetType().GetProperties())
            {
                var field = RenderingModel.Fields.SingleOrDefault(f => f.Name == prop.Name);
                if (field != null && field.ValueType == prop.PropertyType)
                {
                    prop.SetValue(Form, field.Value, null);
                }
            }

            base.OnSubmit();
        }

        protected IHtmlString FieldFor(Expression<Func<T, object>> field)
        {
            var htmlAttributes = new Dictionary<string, object>();

            return FieldFor(field, htmlAttributes);
        }

        protected IHtmlString FieldFor(Expression<Func<T, object>> fieldSelector, object htmlAttributes)
        {
            var prop = GetProperty(fieldSelector);
            var dictionary = Functions.ObjectToDictionary(htmlAttributes);

            var field = RenderingModel.Fields.Single(f => f.Name == prop.Name);

            return FormRenderer.FieldFor(field);
        }

        protected IHtmlString NameFor(Expression<Func<T, object>> fieldSelector)
        {
            var prop = GetProperty(fieldSelector);
            var field = RenderingModel.Fields.Single(f => f.Name == prop.Name);

            return FormRenderer.NameFor(field);
        }

        public PropertyInfo GetProperty(string name)
        {
            return GetType().GetProperty(name);
        }

        public PropertyInfo GetProperty(Expression<Func<T, object>> field)
        {
            MemberExpression memberExpression;

            switch (field.Body.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    var unaryExpression = (UnaryExpression)field.Body;
                    memberExpression = (MemberExpression)unaryExpression.Operand;

                    break;

                default:
                    memberExpression = field.Body as MemberExpression;

                    break;
            }

            return (PropertyInfo)memberExpression.Member;
        }
    }
}
