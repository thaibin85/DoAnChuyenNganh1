using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class ProductCategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        Uri BaseAddress = new Uri("https://localhost:44375/api");
        private readonly HttpClient _client;
        public ProductCategoryController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseAddress;
        }
        // GET: Admin/ProductCategory
        public ActionResult Index()
        {
            //var items = db.ProductCategories;
            List<ProductCategory> productCategoriesList = new List<ProductCategory>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/productcategories").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                productCategoriesList = JsonConvert.DeserializeObject<List<ProductCategory>>(data);
            }
            return View(productCategoriesList);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.ProductCategories.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Edit(int id)
        {
            //var item = db.ProductCategories.Find(id);
            ProductCategory productCategory = new ProductCategory();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/productcategories/" + id).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                productCategory = JsonConvert.DeserializeObject<ProductCategory>(data);
            }
            return View(productCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductCategory model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                db.ProductCategories.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}