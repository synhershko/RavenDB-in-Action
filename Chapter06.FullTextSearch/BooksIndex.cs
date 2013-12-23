using System.Linq;
using Chapter06.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Chapter06.FullTextSearch
{
    public class BooksIndex : AbstractIndexCreationTask<Book>
    {
        public BooksIndex()
        {
            Map = books => from book in books
                           select new
                                      {
                                          book.Title,
                                          book.Author,
                                          AuthorUntokenized = book.Author,
                                          book.Price,
                                          book.Pages
                                      };

            Index(x => x.Title, FieldIndexing.Analyzed);
            Index(x => x.Author, FieldIndexing.Analyzed);
            Analyze("AuthorUntokenized", "KeywordAnalyzer");

            Sort(x => x.Price, SortOptions.Double);
            Sort(x => x.Pages, SortOptions.Int);
        }
    }
}
