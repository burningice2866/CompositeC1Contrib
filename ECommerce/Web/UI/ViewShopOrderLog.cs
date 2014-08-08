using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Composite.Data;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce.Web.UI
{
    public class ViewShopOrderLog : Page
    {
        protected Literal lit;

        protected IShopOrder ShopOrder;

        protected override void OnInit(EventArgs e)
        {
            using (var data = new DataConnection())
            {
                ShopOrder = data.Get<IShopOrder>().Single(o => o.Id == Request.QueryString["id"]);
            }

            var logFile = Path.Combine(ECommerce.RootPath, String.Format("log.{0}.txt", ShopOrder.Id));
            if (!File.Exists(logFile))
            {
                lit.Text = "No logfile found for selected order";
            }
            else
            {
                var lines = File.ReadAllLines(logFile).Select(HttpUtility.HtmlEncode);

                lit.Text = String.Join("<br />", lines);
            }

            base.OnInit(e);
        }

        protected void OnBack(object sender, EventArgs e)
        {
            Response.Redirect("ViewShopOrder.aspx?id=" + ShopOrder.Id);
        }
    }
}
