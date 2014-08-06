
using CompositeC1Contrib.ECommerce.Data.Types;

namespace CompositeC1Contrib.ECommerce
{
    public interface IOrderProcessor
    {
        void HandleHandlerContinue(IShopOrder order);
        void PostProcessOrder(IShopOrder order);
    }
}
