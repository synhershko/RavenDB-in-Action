using System.Linq;
using System.Web.Mvc;
using Chapter02Bookstore.Models;
using Raven.Client;

namespace Chapter02Bookstore.Controllers
{
    public class BooksController : RavenController
    {
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Book());
        }

        [HttpPost]
        public ActionResult Create(Book book)
        {
            if (!ModelState.IsValid)
            {
                return View(book);
            }

            RavenSession.Store(book);

            return Json("Book was successfully added and was assigned the ID " + book.Id, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Show(int id)
        {
            var book = RavenSession.Load<Book>(id);
            if (book == null)
                return HttpNotFound("Requested book wasn't found in the system");

            return Json(book, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var book = RavenSession.Load<Book>(id);
            if (book == null)
                return HttpNotFound("Requested book wasn't found in the system");

            return View(book);
        }

        [HttpPost]
        public ActionResult Edit(int id, Book input)
        {
            var book = RavenSession.Load<Book>(id);
            if (book == null)
                return HttpNotFound("Requested book wasn't found in the system");

            book.Title = input.Title;
            book.Author = input.Author;
            book.Description = input.Description;
            book.Price = input.Price;
            book.YearPublished = input.YearPublished;
            // And so on...

            return Json("Book was edited successfully", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult DeleteConfirmed(int id)
        {
            var book = RavenSession.Load<Book>(id);            
            if (book == null)
                return HttpNotFound("Requested book wasn't found in the system");
            RavenSession.Delete(book);

            return Json("Deleted successfully", JsonRequestBehavior.AllowGet);
        }

        private const int PageSize = 10;

        public ActionResult ListAll(int? page)
        {
            RavenQueryStatistics stats;
            var books = RavenSession.Query<Book>()
                .Statistics(out stats)
                .OrderByDescending(x => x.YearPublished)
                .Skip(PageSize * page ?? 0)
                .Take(PageSize)
                .ToList();

            ViewBag.Title = stats.TotalResults + " books found";

            return View("List", books);
        }


        public ActionResult ListByPriceLimit(double price)
        {
            RavenQueryStatistics stats;
            var books = RavenSession.Query<Book>()
                .Statistics(out stats)
                .Where(x => x.Price <= price)
                .OrderBy(x => x.Price)
                .ToList();

            ViewBag.Title = stats.TotalResults + " books found";

            return View("List", books);
        }

        public ActionResult ListByYear(int year)
        {
            RavenQueryStatistics stats;
            var books = RavenSession.Query<Book>()
                .Statistics(out stats)
                .Where(x => x.YearPublished == year)
                .ToList();

            ViewBag.Title = stats.TotalResults + " books found";

            return View("List", books);
        }

        public ActionResult ListByDepartment(string department)
        {
            RavenQueryStatistics stats;
            var books = RavenSession.Query<Book>()
                .Statistics(out stats)
                .Where(x => x.Departments.Any(y => y.Equals(department)))
                .ToList();

            ViewBag.Title = stats.TotalResults + " books found";

            return View("List", books);
        }
    }
}