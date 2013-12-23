using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Chapter06.Reporting
{
    public class Orders_ByCustomer : AbstractIndexCreationTask<Order, Orders_ByCustomer.ReduceResult>
    {
        public class ReduceResult
        {
            public string CustomerId { get; set; }
            public int NumberOfOrders { get; set; }
            public int AvgNumberOfItems { get; set; }
            public double AvgPrice { get; set; }
            public double HighestPrice { get; set; }
            public double LowestPrice { get; set; }
        }

        public Orders_ByCustomer()
        {
            Map = orders => from order in orders
                            select new
                                       {
                                           order.CustomerId,
                                           NumberOfOrders = 1,
                                           AvgNumberOfItems = order.NumberOfItems,
                                           AvgPrice = order.TotalPrice,
                                           HighestPrice = int.MinValue,
                                           LowestPrice = int.MaxValue,
                                       };

            Reduce = results => from r in results
                                group r by r.CustomerId
                                into g
                                let ordersCount = g.Sum(x => x.NumberOfOrders)
                                select new ReduceResult
                                           {
                                               CustomerId = g.Key,
                                               NumberOfOrders = ordersCount,
                                               AvgNumberOfItems = g.Sum(x => x.AvgNumberOfItems) / ordersCount, // RavenDB doesn't support Average() directly for performance reasons
                                               AvgPrice = g.Sum(x => x.AvgPrice) / ordersCount,
                                               HighestPrice = g.Max(x => x.AvgPrice),
                                               LowestPrice = g.Min(x => x.AvgPrice),
                                           };
            
            Sort(x => x.AvgNumberOfItems, SortOptions.Int);
            Sort(x => x.AvgPrice, SortOptions.Double);
            Sort(x => x.HighestPrice, SortOptions.Double);
            Sort(x => x.LowestPrice, SortOptions.Double);
        }
    }
}
