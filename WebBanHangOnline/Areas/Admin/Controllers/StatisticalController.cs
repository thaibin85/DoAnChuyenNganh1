using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebBanHangOnline.Models;
using static WebBanHangOnline.ApiControllers.StatisticalController;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    public class StatisticalController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/Statistical
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Month()
        {
            return View();
        }
        public async Task<ActionResult> Month(int? selectedYear)
        {
            try
            {
                // Kiểm tra nếu các tham số selectedYear và selectedMonth không null
                if (selectedYear.HasValue)
                {
                    // Gọi API bằng HttpClient
                    var apiEndpoint = $"https://localhost:44375/api/Statistical/Monthly/{selectedYear}";

                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetStringAsync(apiEndpoint);

                        // Deserializing JSON response to the model
                        var dataFromApi = JsonConvert.DeserializeObject < List <MonthlyRevenue>>(response);

                        // Truyền dữ liệu đến view
                        return Json(dataFromApi, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    // Nếu có ít nhất một tham số là null, trả về dữ liệu mặc định hoặc thông báo lỗi tùy ý
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching data from the API: {ex.ToString()}");
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate)
        {
            var query = from o in db.Orders
                        join od in db.OrderDetails
                        on o.Id equals od.OrderId
                        join p in db.Products
                        on od.ProductId equals p.Id
                        select new
                        {
                            CreatedDate = o.CreatedDate,
                            Quantity = od.Quantity,
                            Price = od.Price,
                            OriginalPrice = p.OriginalPrice
                        };
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime startDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.CreatedDate >= startDate);
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.CreatedDate < endDate);
            }

            var result = query.GroupBy(x => DbFunctions.TruncateTime(x.CreatedDate)).Select(x => new
            {
                Date = x.Key.Value,
                TotalBuy = x.Sum(y => y.Quantity * y.OriginalPrice),
                TotalSell = x.Sum(y => y.Quantity * y.Price),
            }).Select(x => new
            {
                Date = x.Date,
                DoanhThu = x.TotalSell,
                LoiNhuan = x.TotalSell - x.TotalBuy
            });
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);
        }

    }
}