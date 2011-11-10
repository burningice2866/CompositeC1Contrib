using System;
using System.Collections.Generic;
using System.Linq;

using Composite.C1Console.Security;
using Composite.Data;

using CompositeC1Contrib.FormBuilder.Data.Types;

namespace CompositeC1Contrib.FormBuilder.ElementProviders.Tokens
{
    public class FormFolderAncestorProvider : ISecurityAncestorProvider
    {
        public IEnumerable<EntityToken> GetParents(EntityToken entityToken)
        {
            var fieldFolderToken = entityToken as FormFolderEntityToken;
            if (fieldFolderToken != null)
            {
                var form = DataFacade.GetData<IForm>().Single(f => f.Id == Guid.Parse(fieldFolderToken.Id));

                yield return form.GetDataEntityToken();
            }
        }
    }
}
