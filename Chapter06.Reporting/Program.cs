using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Document;

namespace Chapter06.Reporting
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to the RavenDB server; assumes one is running locally
            var store =
                new DocumentStore {Url = "http://localhost:8080", DefaultDatabase = "Chapter06.Reporting"}.Initialize();

            new Orders_ByCustomer().Execute(store);
            new OrdersSimpleMapIndex().Execute(store);

            PutTestData(store);

            Console.WriteLine("Map/Reduce example - one M/R index can answer so much...");
            using (var session = store.OpenSession())
            {
                var results = session.Query<Orders_ByCustomer.ReduceResult, Orders_ByCustomer>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfNow()) // Since we just created the index, for the purposes of this demo we want to wait for it to complete indexing
                    .OrderBy(x => x.CustomerId) // Sort results by customer ID
                    .ToList();
                Console.WriteLine("Customer Id\tOrders count\tAvg Items #\tAvg price\tHighest price\tLowest price");
                foreach (var r in results)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", r.CustomerId, r.NumberOfOrders, r.AvgNumberOfItems, r.AvgPrice, r.HighestPrice, r.LowestPrice);
                }
            }
            WaitForUserToContinue();

            Console.WriteLine("Doing dynamic aggregations using Facets...");
            using (var session = store.OpenSession())
            {
                var results = session.Query<Order, OrdersSimpleMapIndex>()
                       .AggregateBy(x => x.CustomerId)
                            .AverageOn(x => x.NumberOfItems)
                            .MaxOn(x => x.TotalPrice)
                            .MinOn(x => x.TotalPrice)
                            .SumOn(x => x.TotalPrice)
                       .AndAggregateOn(x => x.Currency)
                            .AverageOn(x => x.NumberOfItems)
                            .MaxOn(x => x.TotalPrice)
                            .MinOn(x => x.TotalPrice)
                            .SumOn(x => x.TotalPrice)
                       .ToList()
                    ;

                foreach (var facetResult in results.Results)
                {
                    Console.WriteLine(facetResult.Key + ": ");
                    
                    foreach (var facetValue in facetResult.Value.Values)
                    {
                        Console.WriteLine("\t" + facetValue.Range + ":");
                        Console.WriteLine("\t\t Number of items: Average="+ facetValue.Average);
                        Console.WriteLine("\t\t Total price: Sum=" + facetValue.Sum + ", Min=" + facetValue.Min + ", Max=" + facetValue.Max);
                    }
                }
            }
        }

        private static void WaitForUserToContinue()
        {
            Console.WriteLine();
            Console.WriteLine("(press any key to continue the demo)");
            Console.ReadKey();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("                                    ");
        }

        private static void PutTestData(IDocumentStore store)
        {
            // Put some test data in; we specify the IDs explicitly to avoid data duplication during multiple consecutive runs
            using (var session = store.OpenSession())
            {
                session.Store(new Order
                                  {
                                      Id = "orders/1",
                                      CustomerId = "customers/5",
                                      CreatedAt = new DateTime(2012, 12, 05),
                                      NumberOfItems = 2,
                                      TotalPrice = 823.12,
                                      Currency = "USD"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/2",
                                      CustomerId = "customers/10",
                                      CreatedAt = new DateTime(2013, 1, 2),
                                      NumberOfItems = 1,
                                      TotalPrice = 102.99,
                                      Currency = "USD"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/3",
                                      CustomerId = "customers/1",
                                      CreatedAt = new DateTime(2013, 3, 12),
                                      NumberOfItems = 2,
                                      TotalPrice = 35.50,
                                      Currency = "USD"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/4",
                                      CustomerId = "customers/2",
                                      CreatedAt = new DateTime(2013, 4, 1),
                                      NumberOfItems = 5,
                                      TotalPrice = 1052.00,
                                      Currency = "EUR"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/5",
                                      CustomerId = "customers/9",
                                      CreatedAt = new DateTime(2013, 4, 20),
                                      NumberOfItems = 3,
                                      TotalPrice = 15.00,
                                      Currency = "EUR"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/6",
                                      CustomerId = "customers/20",
                                      CreatedAt = new DateTime(2013, 4, 21),
                                      NumberOfItems = 1,
                                      TotalPrice = 14.60,
                                      Currency = "USD"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/7",
                                      CustomerId = "customers/5",
                                      CreatedAt = new DateTime(2013, 6, 11),
                                      NumberOfItems = 4,
                                      TotalPrice = 86.99,
                                      Currency = "EUR"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/8",
                                      CustomerId = "customers/8",
                                      CreatedAt = new DateTime(2013, 07, 08),
                                      NumberOfItems = 10,
                                      TotalPrice = 743.00,
                                      Currency = "USD"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/9",
                                      CustomerId = "customers/5",
                                      CreatedAt = new DateTime(2013, 7, 12),
                                      NumberOfItems = 1,
                                      TotalPrice = 23.10,
                                      Currency = "USD"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/10",
                                      CustomerId = "customers/9",
                                      CreatedAt = new DateTime(2013, 08, 01),
                                      NumberOfItems = 1,
                                      TotalPrice = 71.24,
                                      Currency = "EUR"
                                  });

                session.Store(new Order
                                  {
                                      Id = "orders/11",
                                      CustomerId = "customers/12",
                                      CreatedAt = new DateTime(2014, 01, 18),
                                      NumberOfItems = 2,
                                      TotalPrice = 75.98,
                                      Currency = "USD"
                                  });

                session.SaveChanges();
            }
        }
    }
}
