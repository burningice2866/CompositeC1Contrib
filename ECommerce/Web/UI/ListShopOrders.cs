using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web.UI
{
    public class ListShopOrders : Page
    {
        protected Repeater rpt;

        protected override void OnLoad(EventArgs e)
        {
            Bind();

            base.OnLoad(e);
        }

        private void Bind()
        {
            var status = (PaymentStatus)int.Parse(Request.QueryString["status"]);

            using (var data = new DataConnection())
            {
                var orders =
                    data.Get<IShopOrder>()
                        .Where(o => o.PaymentStatus == (int)status)
                        .OrderByDescending(o => o.CreatedOn)
                        .Take(100)
                        .ToList();

                rpt.DataSource = orders;
                rpt.DataBind();
            }
        }

        protected void OnRefresh(object sender, EventArgs e)
        {
            Bind();
        }
    }
}
