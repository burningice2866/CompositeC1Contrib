using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web.UI
{
    public class ViewShopOrderLog : Page
    {
        protected Repeater rpt;

        protected IShopOrder ShopOrder;

        protected override void OnInit(EventArgs e)
        {
            using (var data = new DataConnection())
            {
                ShopOrder = data.Get<IShopOrder>().Single(o => o.Id == Request.QueryString["id"]);

                var logEntries = data.Get<IShopOrderLog>().Where(l => l.ShopOrderId == ShopOrder.Id).OrderBy(l => l.Timestamp).ToList();

                rpt.DataSource = logEntries;
                rpt.DataBind();
            }

            base.OnInit(e);
        }

        protected void OnBack(object sender, EventArgs e)
        {
            Response.Redirect("ViewShopOrder.aspx?id=" + ShopOrder.Id);
        }
    }
}
