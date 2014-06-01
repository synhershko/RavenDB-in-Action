namespace Chapter7.Sharding.Models
{
    public class Order
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public double TotalPrice { get; set; }
    }
}
