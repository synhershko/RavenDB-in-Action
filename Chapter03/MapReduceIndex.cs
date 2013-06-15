using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chapter02Bookstore.Models;
using Raven.Client.Indexes;

namespace Chapter03
{
    public class MapReduceIndex : AbstractIndexCreationTask<Book, MapReduceIndex.ReduceResult>
    {
        public class ReduceResult
        {
            public int Year { get; set; }
            public int Count { get; set; }
        }

        public MapReduceIndex()
        {
            Map = books => from book in books
                           select new
                                      {
                                          Year = book.YearPublished,
                                          Count = 1,
                                      };

            Reduce = results => from r in results
                                group r by r.Year
                                into g
                                select new
                                           {
                                               Year = g.Key,
                                               Count = g.Sum(x => x.Count),
                                           };
        }
    }
}
