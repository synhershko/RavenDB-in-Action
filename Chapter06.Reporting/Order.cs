using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chapter06.Reporting
{
    public class Order
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int NumberOfItems { get; set; }
        public double TotalPrice { get; set; }
        public string Currency { get; set; }
    }
}
