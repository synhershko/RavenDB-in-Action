using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Chapter06.Reporting
{
    public class OrdersSimpleMapIndex : AbstractIndexCreationTask<Order>
    {
        public OrdersSimpleMapIndex()
        {
            Map = orders => from order in orders
                            select new
                                       {
                                           order.CustomerId,
                                           order.CreatedAt,
                                           order.NumberOfItems,
                                           order.TotalPrice,
                                           order.Currency,
                                       };

            Sort(x => x.NumberOfItems, SortOptions.Int);
            Sort(x => x.TotalPrice, SortOptions.Double);
        }
    }
}
