using System;
using System.IO;
using System.Linq;
using Raven.Abstractions.Commands;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;
using Raven.Json.Linq;

namespace Chapter01
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public double Price { get; set; }
        public int YearPublished { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var docStore = new DocumentStore
                                          {
                                              Url = "http://localhost:8080",
                                              DefaultDatabase = "RavenDB_In_Action",
                                          };
            docStore.Initialize();


//            using (var session = docStore.OpenSession())
//            {
//                session.Store(new Book
//                {
//                    Title = "Harry Potter",
//                    Author = "J.K. Rowling",
//                    Price = 32.99,
//                    YearPublished = 2001,
//                });
//
//                session.SaveChanges();
//            }

            using (var session = docStore.OpenSession())
            {
                var book = session.Load<Book>("books/1");
                Console.WriteLine(book.Title);
            }

            using (var session = docStore.OpenSession())
            {
                var book = session.Load<Book>("books/1");
                book.Price = 10.99; // price was now updated in-memory, and not in the database
                session.SaveChanges(); // only now the price change was saved to the database
            }

            using (var session = docStore.OpenSession())
            {
                var book = session.Load<Book>("books/1");
                session.Delete(book); // marks the document to be deleted
                session.SaveChanges(); // performs the actual deletion
            }

            using (var session = docStore.OpenSession())
            {
                var book = session.Load<Book>("books/1");
                session.Delete(book); // marks the document to be deleted

                session.Advanced.Defer(new DeleteCommandData { Key = "posts/1234" });
                session.SaveChanges(); // performs the actual deletion
            }

            using (var session = docStore.OpenSession())
            {
                // this will only get the first page of books,
                // if there are more than 128 books in the system!
                var allBooks = session.Query<Book>().ToList();

                var jkRowlingBooks = session.Query<Book>()
                                            .Where(x => x.Author.Equals("J.K. Rowling"))
                                            .ToList();

                var cheapBooks = session.Query<Book>()
                                        .Where(x => x.Price < 10)
                                        .ToList();

                //session.Query<Book>().Customize(x => x.WaitForNonStaleResultsAsOf())
            }
        }
    }
}
