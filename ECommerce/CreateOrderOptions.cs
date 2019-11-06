using CompositeC1Contrib.ECommerce.Configuration;

namespace CompositeC1Contrib.ECommerce
{
    public class CreateOrderOptions
    {
        public string OrderIdPrefix { get; set; }
        public int MinimumOrderIdLength { get; set; }
        public decimal TotalAmount { get; set; }
        public Currency Currency { get; set; }
        public string CustomData { get; set; }

        public CreateOrderOptions()
        {
            var section = ECommerceSection.GetSection();

            OrderIdPrefix = section.OrderIdPrefix;
            MinimumOrderIdLength = section.MinimumOrderIdLength;
            Currency = section.DefaultCurrency;
        }
    }
}
