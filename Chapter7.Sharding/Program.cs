using System;
using System.Collections.Generic;
using Chapter7.Sharding.Models;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Shard;

namespace Chapter7.Sharding
{
    class Program
    {
        static void Main(string[] args)
        {
            // Init
            Console.WriteLine("Please start 3 instances of RavenDB. This sample expects them to listen on ports 8080, 8081 and 8082.");
            Console.WriteLine("Feel free to change the sample to use other ports, or add more servers to the list.");
            Console.WriteLine("For easier experience, run them in-memory by running Raven.Server.exe /ram");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // Blind sharding
            Console.WriteLine();            
            Console.WriteLine("Starting with Blind Sharding. You will see documents spreading evenly between servers.");
            Console.WriteLine("You will notice order documents will be written to shards without taking into account the shard with the document representing the user created them. Those will be highlighted.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            var shards = new Dictionary<string, IDocumentStore>
            {
                {"Users1", new DocumentStore{Url = "http://localhost:8080", DefaultDatabase = "Chapter7-BlindSharding"}},
                {"Users2", new DocumentStore{Url = "http://localhost:8081", DefaultDatabase = "Chapter7-BlindSharding"}},
                {"Users3", new DocumentStore{Url = "http://localhost:8082", DefaultDatabase = "Chapter7-BlindSharding"}},
            };
            using (var docStore = new ShardedDocumentStore(new ShardStrategy(shards)))
            {
                docStore.Initialize();
                PushDataToDocumentStore(docStore);
            }

            // Data driven sharding
            Console.WriteLine();
            Console.WriteLine("Now moving to using data driven sharding using a well-defined sharding strategy.");
            Console.WriteLine("The sharding strategy used will send User documents to shards based on the first letter of the username.");
            Console.WriteLine("You can now see the order documents will always reside on the same shard as their respective User document.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            shards = new Dictionary<string, IDocumentStore>
            {
                {"Users1", new DocumentStore{Url = "http://localhost:8080", DefaultDatabase = "Chapter7-SmarterSharding"}},
                {"Users2", new DocumentStore{Url = "http://localhost:8081", DefaultDatabase = "Chapter7-SmarterSharding"}},
                {"Users3", new DocumentStore{Url = "http://localhost:8082", DefaultDatabase = "Chapter7-SmarterSharding"}},
            };

            // Our data-aware sharding strategy.
            // The User documents will be sharded based on the value in their Username property
            // using a translator function tells RavenDB how to resolve the shard ID from
            // the value of the document property holding the routing key.
            // Then, Order documents will be sent to the shards where the User they are associated
            // with is stored.
            var shardingStrategy = new ShardStrategy(shards)
                .ShardingOn<User>(x => x.Username,
                    username =>
                    {
                        username = username.ToLowerInvariant();
                        if (username[0] >= 'a' && username[0] < 'm')
                            return "Users1";
                        if (username[0] >= 'm' && username[0] < 'q')
                            return "Users2";
                        // otherwise (starting with q to z, or a non alpha-bet
                        return "Users3";
                    })
                .ShardingOn<Order>(x => x.UserId);

            using (var docStore = new ShardedDocumentStore(shardingStrategy))
            {
                docStore.Initialize();
                PushDataToDocumentStore(docStore);
            }

        }

        private static void PushDataToDocumentStore(ShardedDocumentStore docStore)
        {
            var users = new List<User>();

            for (var i = 0; i < 30; i++)
            {
                using (var session = docStore.OpenSession())
                {
                    var user =
                        Builder<User>.CreateNew()
                            .With(x => x.Id = null)
                            .With(x => x.Username = GetRandom.FirstName() + "." + GetRandom.LastName())
                            .Build();
                    session.Store(user);
                    Console.WriteLine("Storing {0}...", user.Id);
                    session.SaveChanges();

                    users.Add(user); // storing user objects so we an add orders for them
                }
            }

            for (var i = 0; i < 10; i++)
            {
                using (var session = docStore.OpenSession())
                {
                    var order = Builder<Order>.CreateNew()
                        .With(x => x.UserId = users[GetRandom.Int(0, users.Count - 1)].Id)
                        .With(x => x.Id = null)
                        .With(x => x.TotalPrice = GetRandom.PositiveDouble())
                        .Build();
                    session.Store(order);
                    var color = Console.ForegroundColor;

                    if (!order.UserId.StartsWith(order.Id.Substring(0, 7)))
                        Console.ForegroundColor = ConsoleColor.Red; 
                   
                    Console.WriteLine("Storing {0} for user {1}...", order.Id, order.UserId);
                    Console.ForegroundColor = color;
                    session.SaveChanges();
                }
            }
        }
    }
}
