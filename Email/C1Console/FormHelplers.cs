﻿using System;
using System.Collections.Generic;
using System.Linq;

using Composite.Data;

using CompositeC1Contrib.Email.Data.Types;

namespace CompositeC1Contrib.Email.C1Console
{
    public class FormHelplers
    {
        public static IEnumerable<Type> GetEmbedableFieldsTypes(string key)
        {
            using (var data = new DataConnection())
            {
                return
                    data.Get<IMailTemplate>()
                        .Where(t => t.Key == key && !String.IsNullOrEmpty(t.ModelType))
                        .Select(t => Type.GetType(t.ModelType));
            }
        }
    }
}
