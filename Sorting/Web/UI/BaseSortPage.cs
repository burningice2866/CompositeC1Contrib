using System;
using System.Web.UI;
using Composite.Data;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class BaseSortPage : Page
    {
        protected static string hashId(IData data)
        {
            return data.DataSourceId.GetKeyValue().GetHashCode().ToString().Replace("-", String.Empty);
        }
    }
}
