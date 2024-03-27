using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reportes0DB.Models;
using Rotativa.AspNetCore;

namespace Reportes0DB.Controllers
{
    public class ReportesController : Controller
    {
        private readonly Reportes0DBContext _context;

        public ReportesController(Reportes0DBContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GenerarReporte(string customerID)
        {

            var ordenes = await _context.Orders
                .Include(s => s.Customer)
                .Include(s => s.OrderDetails)
                .ThenInclude(s => s.Product)
                .ThenInclude(s => s.Category)
                .Include(s => s.Employee)
                .Where(s => s.CustomerId == customerID)
                .OrderByDescending(s => s.OrderDate)
                .ToListAsync();
            return new ViewAsPdf("ReporteOrdenes", ordenes)
            {
                //..
            };
        }
    }
}
