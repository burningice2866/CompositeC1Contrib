using CompositeC1Contrib.ECommerce.Configuration;

namespace CompositeC1Contrib.ECommerce
{
    public class OrderCreationSettings
    {
        public string OrderIdPrefix { get; set; }
        public int MinimumOrderIdLength { get; set; }
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }
        public string CustomData { get; set; }

        public OrderCreationSettings()
        {
            var section = ECommerceSection.GetSection();

            OrderIdPrefix = section.OrderIdPrefix;
            MinimumOrderIdLength = section.MinimumOrderIdLength;
            Currency = section.DefaultCurrency;
        }
    }
}
