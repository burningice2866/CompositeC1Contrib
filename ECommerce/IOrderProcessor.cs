using System.Web;

using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public interface IOrderProcessor
    {
        string GenerateNextOrderId(OrderCreationSettings settings);
        string HandleContinue(HttpContextBase context, IShopOrder order);
        void PostProcessOrder(IShopOrder order);
    }
}
