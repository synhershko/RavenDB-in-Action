using System.Web.Mvc;
using Raven.Client;

namespace Chapter02Bookstore.Controllers
{
    public abstract class RavenController : Controller
    {
        public IDocumentSession RavenSession { get; set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RavenSession = MvcApplication.TheDocumentStore.OpenSession();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            using (RavenSession)
            {
                if (filterContext.Exception == null)
                    RavenSession.SaveChanges();
            }
        }
    }
}