using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Web.Http;

namespace WebApiTest.Controllers
{
    public class ProductController : ApiController
    {
        private readonly IDbConnection _db;
        public IActionResult Index()
        {
            return View();
        }
    }
}
