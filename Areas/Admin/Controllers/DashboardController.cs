using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Models;
using Shopping_Tutorial.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;

        public DashboardController(DataContext context)
        {
            _dataContext = context;
        }

        public IActionResult Index()
        {
            ViewBag.CountProduct = _dataContext.Products.Count();
            ViewBag.CountOrder = _dataContext.Orders.Count();
            ViewBag.CountCategory = _dataContext.Categories.Count();
            ViewBag.CountUser = _dataContext.Users.Count();
            return View();
        }

        [HttpPost]
        [Route("GetChartData")]
        public IActionResult GetChartData()
        {
            var data = _dataContext.Statisticals
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }

        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data = _dataContext.Statisticals
                .Where(s => s.DateCreated >= startDate && s.DateCreated <= endDate)
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }

        [HttpPost]
        [Route("FilterData")]
        public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _dataContext.Statisticals.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.DateCreated >= fromDate);

            if (toDate.HasValue)
                query = query.Where(s => s.DateCreated <= toDate);

            var data = query
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }
    }
}
