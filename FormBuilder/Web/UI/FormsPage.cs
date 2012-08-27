using System;
using System.Linq.Expressions;
using System.Web;

namespace CompositeC1Contrib.FormBuilder.Web.UI
{
    public abstract class FormsPage<T> : BaseFormPage<T> where T : BaseForm
    {
        protected override Type ResolveFormType()
        {
            return typeof(T);
        }

        public IHtmlString FieldFor(Expression<Func<T, object>> field)
        {
            return Form.FieldFor<T>(field);
        }
    }
}
