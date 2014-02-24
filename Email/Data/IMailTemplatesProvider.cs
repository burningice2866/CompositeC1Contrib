using System;
using System.Collections.Generic;

namespace CompositeC1Contrib.Email.Data
{
    public interface IMailTemplatesProvider
    {
        IEnumerable<Tuple<string, Type>> GetTemplates();
    }
}
