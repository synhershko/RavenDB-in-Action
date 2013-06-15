using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chapter02Bookstore.Models;
using Raven.Client.Indexes;

namespace Chapter03
{
    public class SampleIndex : AbstractIndexCreationTask<Book>
    {
        public SampleIndex()
        {
            Map = docs => from doc in docs
                          select new
                                     {
                                         doc.Title,
                                         doc.Author,
                                         doc.Price,
                                         Year = doc.YearPublished,
                                     };
        }
    }
}
