namespace NextApi.Common.Ordering
{
    /// <summary>
    /// NextApi Order
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// </summary>
        public Order()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Order"/> class.
        /// </summary>
        public Order(OrderOperators orderOperator, string property)
        {
            OrderOperator = orderOperator;
            Property = property;
        }
        
        /// <summary>
        /// Is ascending order
        /// </summary>
        public OrderOperators OrderOperator { get; set; } = OrderOperators.OrderBy;
        
        /// <summary>
        /// Name of property
        /// </summary>
        public string Property { get; set; }
    }
}
