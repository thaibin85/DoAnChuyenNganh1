using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    //[Authorize(Roles = "Admin,Employee")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        Uri BaseAddress = new Uri("https://localhost:44375/api");
        private readonly HttpClient _client;
        public ProductsController()
        {
            _client = new HttpClient();
            _client.BaseAddress = BaseAddress;
        }
        List<Product> GetProducts()
        {
            List<Product> productsList = new List<Product>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/products").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                productsList = JsonConvert.DeserializeObject<List<Product>>(data);
            }
            return productsList;
        }
        Product GetProduct(int id)
        {
            Product product = new Product();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/products/" + id).Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                product = JsonConvert.DeserializeObject<Product>(data);
            }
            return product;
        }

        List<ProductCategory> GetProductCategories()
        {
            List<ProductCategory> productCategoriesList = new List<ProductCategory>();
            HttpResponseMessage response = _client.GetAsync(_client.BaseAddress + "/productcategories").Result;
            if (response.IsSuccessStatusCode)
            {
                string data = response.Content.ReadAsStringAsync().Result;
                productCategoriesList = JsonConvert.DeserializeObject<List<ProductCategory>>(data);
            }
            return productCategoriesList;
        }
        HttpResponseMessage PutProduct(Product model)
        {
            string data = JsonConvert.SerializeObject(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");
            HttpResponseMessage response = _client.PutAsync(_client.BaseAddress + "/Products/", content).Result;
            return response;
        }
        // GET: Admin/Products
        public ActionResult Index(int? page)
        {
            IEnumerable<Product> items = GetProducts().OrderByDescending(c=>c.Id);
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            items = items.ToPagedList(pageIndex, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = page;
            return View(items);
        }

        public ActionResult Add()
        {
            ViewBag.ProductCategory = new SelectList(GetProductCategories().ToList(), "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model, List<string> Images, List<int> rDefault)
        {
            if (ModelState.IsValid)
            {
                if (Images != null && Images.Count > 0)
                {
                    for (int i = 0; i < Images.Count; i++)
                    {
                        if (i + 1 == rDefault[0])
                        {
                            model.Image = Images[i];
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = true
                            });
                        }
                        else
                        {
                            model.ProductImage.Add(new ProductImage
                            {
                                ProductId = model.Id,
                                Image = Images[i],
                                IsDefault = false
                            });
                        }
                    }
                }
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(model.SeoTitle))
                {
                    model.SeoTitle = model.Title;
                }
                if (string.IsNullOrEmpty(model.Alias))
                    model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                string data = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(data, Encoding.UTF8,"application/json");
                HttpResponseMessage response = _client.PostAsync(_client.BaseAddress + "/Products/PostProduct", content).Result;
                return RedirectToAction("Index");
            }
            ViewBag.ProductCategory = new SelectList(GetProductCategories().ToList(), "Id", "Title");
            return View(model);
        }


        public ActionResult Edit(int id)
        {
            ViewBag.ProductCategory = new SelectList(GetProductCategories().ToList(), "Id", "Title");
            Product product = GetProduct(id);   

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.Alias = WebBanHangOnline.Models.Common.Filter.FilterChar(model.Title);
                var response = PutProduct(model);
                if(response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = GetProduct(id);
            if (item != null)
            {
                var checkImg = item.ProductImage.Where(x => x.ProductId == item.Id);
                if (checkImg != null)
                {
                    foreach(var img in checkImg)
                    {
                        db.ProductImages.Attach(img);
                        db.ProductImages.Remove(img);
                        db.SaveChanges();
                    }
                }
                HttpResponseMessage response = _client.DeleteAsync(_client.BaseAddress + "/Products/" + id).Result;
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsActive(int id)
        {
            var item = GetProduct(id);
            if (item != null)
            {
                item.IsActive = !item.IsActive;
                PutProduct(item);
                return Json(new { success = true, isAcive = item.IsActive });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult IsHome(int id)
        {
            var item = GetProduct(id);
            if (item != null)
            {
                item.IsHome = !item.IsHome;
                PutProduct(item);
                return Json(new { success = true, IsHome = item.IsHome });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult IsSale(int id)
        {
            var item = GetProduct(id);
            if (item != null)
            {
                item.IsSale = !item.IsSale;
                PutProduct(item);
                return Json(new { success = true, IsSale = item.IsSale });
            }

            return Json(new { success = false });
        }
    }
}