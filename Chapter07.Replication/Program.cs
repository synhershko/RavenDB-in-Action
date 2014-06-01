using System;
using Raven.Client.Document;

namespace Chapter07.Replication
{
    class Program
    {
        static void Main(string[] args)
        {
            // Before you run this sample, make sure to have at least 2 instances of RavenDB running,
            // and create a database named "test" with the Replication Bundle enabled on all of them.
            // Follow the instructions in the book if you are not sure how to go about that.

            var MasterServerUrl = "http://localhost:8080";

            var documentStore = new DocumentStore
            {
                Url = MasterServerUrl,
                DefaultDatabase = "test",
            };
            documentStore.Initialize();

//            documentStore.Conventions.FailoverBehavior = FailoverBehavior.AllowReadsFromSecondariesAndWritesToSecondaries;

            string docId;
            using (var session = documentStore.OpenSession())
            {
                var obj = new Person {Name = "Somebody"};
                session.Store(obj);
                session.SaveChanges();

                docId = obj.Id;
            }
            
            Console.WriteLine("We have now written one document to the database, using the Master.");
            Console.WriteLine("You can go and verify it was replicated to all the Slaves by using the Management Studio.");
            Console.WriteLine("Press any key when you are ready to continue");
            Console.ReadKey();

            Console.WriteLine("Ok. Now please kill the Master server (the one listening on {0})", MasterServerUrl);
            Console.WriteLine("Press any key when you are ready to continue");
            Console.ReadKey();

            using (var session = documentStore.OpenSession())
            {
                var obj = session.Load<Person>(docId);
                Console.WriteLine("Successfully loaded {0}!", obj.Name);
            }
        }

        public class Person
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
