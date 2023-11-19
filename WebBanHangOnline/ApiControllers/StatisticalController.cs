using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using WebBanHangOnline.Models;
using WebBanHangOnline.Models.EF;

namespace WebBanHangOnline.ApiControllers
{
    public class StatisticalController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        [HttpGet]
        [Route("api/Statistical/{month}/{year}")]
        public string GetRevenueOfMonth(int year, int month)
        {
            try
            {
                var tong = db.Orders
                    .Where(c => c.CreatedDate.Year == year && c.CreatedDate.Month == month)
                    .Sum(c => c.TotalAmount);

                return tong.ToString();
            }
            catch (Exception ex)
            {
                // Ghi log hoặc trả về thông báo lỗi chi tiết để kiểm tra
                return $"Error: {ex.Message}";
            }
        }
        [HttpGet]
        [Route("api/Statistical/{year}")]
        public string GetRevenueOfYear(int year)
        {
            try
            {
                var tong = db.Orders
                    .Where(c => c.CreatedDate.Year == year)
                    .Sum(c => c.TotalAmount);

                return tong.ToString();
            }
            catch (Exception ex)
            {
                // Ghi log hoặc trả về thông báo lỗi chi tiết để kiểm tra
                return $"Error: {ex.Message}";
            }
        }
        public class MonthlyRevenue
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        [HttpGet]
        [Route("api/Statistical/Monthly/{year}")]
        public List<MonthlyRevenue> GetRevenueMonthly(int year)
        {
            var groupedOrders = db.Orders
                .Where(o => o.CreatedDate.Year == year)
                .GroupBy(o => o.CreatedDate.Month)
                .ToList();

            var totalMonthly = new List<MonthlyRevenue>();

            foreach (var group in groupedOrders)
            {
                var totalRevenue = group.Sum(c => c.TotalAmount);

                var monthlyRevenue = new MonthlyRevenue
                {
                    TotalRevenue = totalRevenue,
                    Month = group.Key,
                    Year = year
                };

                totalMonthly.Add(monthlyRevenue);
            }

            return totalMonthly;
        }

    }
}
