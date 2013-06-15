using System.Collections.Generic;
using System.Web.Mvc;
using Chapter02Bookstore.Controllers;
using Chapter02Bookstore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Embedded;

namespace Chapter02Bookstore.Tests
{
    public class BooksControllerTests
    {
        [TestMethod]
        public void ReturnsBooksByPriceLimit()
        {
            using (var docStore = new EmbeddableDocumentStore { RunInMemory = true }
                .Initialize()
                )
            {
                using (var session = docStore.OpenSession())
                {
                    // Store test data
                    session.Store(new Book { Title = "Test book", YearPublished = 2013, Price = 12.99 });
                    session.SaveChanges();
                }

                var controller = new BooksController { RavenSession = docStore.OpenSession() };

                var viewResult = (ViewResult)controller.ListByPriceLimit(15);
                var result = viewResult.ViewData.Model as List<Book>;
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Count);

                viewResult = (ViewResult)controller.ListByPriceLimit(10);
                result = viewResult.ViewData.Model as List<Book>;
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);

                controller.RavenSession.Dispose();
            }
        }
    }
}
