
namespace RedmineInterface.Controllers
{
    public class HomeController 
        : System.Web.Mvc.Controller
    {
        public System.Web.Mvc.ActionResult Index()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

    }
}