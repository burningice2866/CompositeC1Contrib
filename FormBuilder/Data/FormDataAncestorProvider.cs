using System;
using System.Linq;

using Composite.Data;
using Composite.Data.Hierarchy;

using CompositeC1Contrib.FormBuilder.Data.Types;

namespace CompositeC1Contrib.FormBuilder.Data
{
    public class FormDataAncestorProvider : IDataAncestorProvider
    {
        public IData GetParent(IData data)
        {
            var formField = data as IFormField;
            if (formField == null)
            {
                throw new ArgumentException("Invalid data type", "data");
            }

            using (var conn = new DataConnection())
            {
                return conn.Get<IForm>().Single(f => f.Id == formField.FormId);
            }
        }
    }
}
