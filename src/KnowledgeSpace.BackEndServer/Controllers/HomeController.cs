using Microsoft.AspNetCore.Mvc;

namespace KnowledgeSpace.BackEndServer.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
