using System;
using System.Linq;
using System.Web.UI;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web.UI
{
    public class ViewShopOrderMaster : MasterPage
    {
        protected IShopOrder ShopOrder;

        protected override void OnInit(EventArgs e)
        {
            using (var data = new DataConnection())
            {
                ShopOrder = data.Get<IShopOrder>().Single(o => o.Id == Request.QueryString["id"]);
            }

            base.OnInit(e);
        }

        protected void OnBack(object sender, EventArgs e)
        {
            Response.Redirect("ListShopOrders.aspx?status=" + ShopOrder.PaymentStatus);
        }

        protected void OnViewLog(object sender, EventArgs e)
        {
            Response.Redirect("ViewShopOrderLog.aspx?id=" + ShopOrder.Id);
        }
    }
}
