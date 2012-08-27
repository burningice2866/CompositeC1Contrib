using System;
using System.Collections.Generic;

namespace CompositeC1Contrib.FormBuilder.Validation
{
    public class FormValidationRule
    {
        public string ValidationMessage { get; set; }
        public IList<string> AffectedFormIds { get; private set; }
        public Func<bool> Rule { get; set; }

        public FormValidationRule(string[] affectedFormIds)
        {
            AffectedFormIds = new List<string>(affectedFormIds);
        }
    }
}
