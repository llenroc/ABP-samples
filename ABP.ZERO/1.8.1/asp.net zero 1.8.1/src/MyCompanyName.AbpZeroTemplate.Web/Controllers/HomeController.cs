using System.Web.Mvc;

namespace MyCompanyName.AbpZeroTemplate.Web.Controllers
{
    public class HomeController : AbpZeroTemplateControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}