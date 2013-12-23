using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Abstractions.Data;
using Raven.Client.Bundles.MoreLikeThis;
using Raven.Client.Connection;
using Chapter06.Models;
using Raven.Client;
using Raven.Client.Document;

namespace Chapter06.FullTextSearch
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Connect to the RavenDB server; assumes one is running locally
            var store =
                new DocumentStore {Url = "http://localhost:8080", DefaultDatabase = "Chapter06.FTS"}.Initialize();

            // Put some test data in; we specify the IDs explicitly to avoid data duplication during multiple consecutive runs
            using (var session = store.OpenSession())
            {
                session.Store(
                    new Book
                    {
                        Author = "Yann Martel",
                        Title = "Life of Pi",
                        Price = 15.00,
                        Pages = 180
                    }, "books/1");
                
                session.Store(
                    new Book
                    {
                        Author = "Dan Brown",
                        Title = "Angels and Demons",
                        Price = 12.99,
                        Pages = 500
                    }, "books/2");

                session.Store(
                    new Book
                        {
                            Author = "J.K. Rowling",
                            Title = "Harry Potter and the Goblet of Fire",
                            Price = 25.00,
                            Pages = 400
                        }, "books/3");

                session.Store(
                    new Book
                    {
                        Author = "Dan Brown",
                        Title = "Deception Point",
                        Price = 11.99,
                        Pages = 350
                    }, "books/4");

                session.SaveChanges();
            }

            Console.WriteLine("Querying for exact match on author \"J. K. Rowling\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book>().Where(x => x.Author == "J.K. Rowling").ToList();
                PrintBookResults(books);
            }


            Console.WriteLine("Trying author \"Rowling\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book>().Where(x => x.Author == "Rowling").ToList();
                PrintBookResults(books);
            }

            Console.Write("Creating an index with analyzed fields... ");
            new BooksIndex().Execute(store);
            Console.WriteLine("Done");
            Console.WriteLine();

            Console.WriteLine("Trying author \"Rowling\" again using the new index...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>().Search(x => x.Author, "Rowling").ToList();
                PrintBookResults(books);
            }

            Console.WriteLine("Combining multiple terms - querying with Search for \"Rowling Brown\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>().Search(x => x.Author, "Rowling Brown").ToList();
                PrintBookResults(books);
            }

            Console.WriteLine("Searching for author \"Brown\" AND title \"angels\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>()
                    .Search(x => x.Author, "Brown")
                    .Search(x => x.Title, "angels", 1, SearchOptions.And)
                    .ToList();
                PrintBookResults(books);
            }

            Console.WriteLine("Searching for author \"Brown\" AND title NOT containing \"angels\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>()
                    .Search(x => x.Author, "Brown")
                    .Search(x => x.Title, "angels", 1, SearchOptions.And | SearchOptions.Not)
                    .ToList();
                PrintBookResults(books);
            }

            Console.WriteLine("Some wildcard tests - searching for author \"Rowli*\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>().Search(x => x.Author, "Rowli*", 1, SearchOptions.Or, EscapeQueryOptions.AllowPostfixWildcard).ToList();
                PrintBookResults(books);
            }

            Console.WriteLine("Searching for author \"bro?n\"...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>().Search(x => x.Author, "bro?n", 1, SearchOptions.Or, EscapeQueryOptions.RawQuery).ToList();
                PrintBookResults(books);
            }

            Console.WriteLine("Searching for a stop-word that does exist in 2 titles - we expect to find nothing...");
            using (var session = store.OpenSession())
            {
                var books = session.Query<Book, BooksIndex>().Search(x => x.Title, "the").ToList();
                PrintBookResults(books);
            }

            // TODO highlights

            Console.WriteLine("Printing authors list straight from the index...");
            var terms = store.DatabaseCommands.GetTerms("BooksIndex", "AuthorUntokenized", null, 128);
            foreach (var term in terms)
            {
                Console.WriteLine("\t" + term);
            }
            Console.WriteLine();
            WaitForUser();
            
            Console.WriteLine("Demonstrating suggestions - querying the index for author \"brwon\"...");
            using (var session = store.OpenSession())
            {
                var query = session.Query<Book>("BooksIndex").Search(x => x.Author, "brwon");
                var results = query.ToList();
                Console.WriteLine("Query for \"brwon\" returned " + results.Count + " results.");
                WaitForUser();

                if (results.Count == 0)
                {
                    Console.WriteLine("Asking for suggestions:");
                    var suggestions = query.Suggest();
                    foreach (var suggestion in suggestions.Suggestions)
                    {
                        Console.WriteLine("\t" + suggestion);
                    }
                }
            }
            WaitForUser();

            Console.WriteLine("MoreLikeThis");
            using (var session = store.OpenSession())
            {
                // TODO
                var list = session.Advanced.MoreLikeThis<Book>("BooksIndex",
                                                               new MoreLikeThisQuery
                                                                   {
                                                                       DocumentId = "books/2",
                                                                       Fields = new[] {"Title", "Author", "Description"},
                                                                       MinimumWordLength = 2,
                                                                   });
                PrintBookResults(list);
            }

            Console.WriteLine("Faceted search...");
            using (var session = store.OpenSession())
            {
                var facets = session.Query<Book>("BooksIndex")
                    // Any Where() clause can go here
                       .ToFacets(new List<Facet>
                                     {
                                         new Facet {Name = "AuthorUntokenized"},
                                         new Facet
                                             {
                                                 Name = "Price_Range",
                                                 Mode = FacetMode.Ranges,
                                                 Ranges = new List<string>
                                                              {
                                                                  "[NULL TO Dx20.0]",
                                                                  "[Dx20.0 TO Dx50.0]",
                                                                  "[Dx50.0 TO Dx200.0]",
                                                                  "[Dx200.0 TO Dx400.0]",
                                                                  "[Dx400.0 TO NULL]",                                                                  
                                                              }
                                             },
                                     });

                foreach (var facetResult in facets.Results)
                {
                    Console.WriteLine("* " + facetResult.Key);
                    foreach (var facetValue in facetResult.Value.Values)
                    {
                        Console.WriteLine("\t" + facetValue.Range + ": " + facetValue.Hits);
                    }
                }
            }
        }


        private static void PrintBookResults(IEnumerable<Book> books)
        {
            if (!books.Any())
            {
                Console.WriteLine("\tNo results were found :(");
            }
            else
            {
                foreach (var book in books)
                {
                    Console.WriteLine("\t\"" + book.Title + "\" by " + book.Author);
                }
            }
            Console.WriteLine();
            WaitForUser();
        }

        private static void WaitForUser()
        {
            Console.WriteLine("(press any key to continue the demo)");
            Console.ReadKey();
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("                                    ");
        }
    }
}
