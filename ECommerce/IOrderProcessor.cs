using System.Net.Http;
using System.Web.Http;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public interface IOrderProcessor
    {
        IHttpActionResult HandleHandlerContinue(IShopOrder order, HttpRequestMessage request, HttpResponseMessage response);
        void PostProcessOrder(IShopOrder order);
    }
}
