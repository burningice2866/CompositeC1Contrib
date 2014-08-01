using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CompositeC1Contrib.Sorting.Web.UI
{
    public class SortMasterPage : MasterPage
    {
        protected Repeater rpt;

        public string CustomJsonDataName { get; set; }
        public string CustomJsonDataValue { get; set; }

        public IEnumerable<SortableItem> SortableItems { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            rpt.DataSource = SortableItems;
            rpt.DataBind();

            base.OnPreRender(e);
        }
    }
}
