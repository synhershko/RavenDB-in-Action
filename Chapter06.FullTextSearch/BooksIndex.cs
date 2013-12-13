using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                           select new {book.Title, book.Author, book.Price, book.Pages};

            Index(x => x.Title, FieldIndexing.Analyzed);
            Index(x => x.Author, FieldIndexing.Analyzed);

            Sort(x => x.Price, SortOptions.Double);
            Sort(x => x.Pages, SortOptions.Int);
        }
    }
}
