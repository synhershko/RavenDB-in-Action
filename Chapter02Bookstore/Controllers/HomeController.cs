using System.Web.Mvc;

namespace Chapter02Bookstore.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return RedirectToAction("ListAll", "Books");
        }

    }
}
