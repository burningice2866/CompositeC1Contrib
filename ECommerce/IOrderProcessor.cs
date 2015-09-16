using System.Net.Http;
using System.Web.Http;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public interface IOrderProcessor
    {
        IHttpActionResult Receipt(IShopOrder order, HttpRequestMessage request);
        void PostProcessOrder(IShopOrder order);
    }
}
