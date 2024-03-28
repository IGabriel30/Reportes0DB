using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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

        public async Task<IActionResult> GenerarReporte(string customerID, string opcion)
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
            if (opcion == "PDF")
            {
                return new ViewAsPdf("ReporteOrdenes", ordenes)
                {
                    // ...
                };
            }
            else if (opcion == "EXCEL")
            {
                using (var package = new ExcelPackage())
                {
                    // Agregamos una nueva hoja de trabajo al paquete
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Ordenes");
                    // Acceder a la columna correspondiente (columna A) y establecer su ancho
                    worksheet.Column(2).Width = 30; // Establecer el ancho de la columna B en 30
                    worksheet.Column(3).Width = 30; // Establecer el ancho de la columna C en 30
                    worksheet.Column(4).Width = 30; // Establecer el ancho de la columna D en 30
                    // Escribimos algunos datos en la hoja de trabajo
                    worksheet.Cells["A1"].Value = "#Orden";
                    worksheet.Cells["B1"].Value = "Cliente";
                    worksheet.Cells["C1"].Value = "Empleado";
                    worksheet.Cells["D1"].Value = "Fecha";

                    int fila = 2;
                    foreach (var item in ordenes)
                    {
                        int columna = 1;
                        worksheet.Cells[fila, columna].Value = item.OrderId;

                        columna++;
                        worksheet.Cells[fila, columna].Value = item.Customer.ContactName;

                        columna++;
                        worksheet.Cells[fila, columna].Value = item.Employee.FirstName;

                        columna++;
                        worksheet.Cells[fila, columna].Style.Numberformat.Format = "yyyy-MM-dd";
                        worksheet.Cells[fila, columna].Value = item.OrderDate;
                        fila++;
                    }
                    var range = worksheet.Cells["A1:D" + fila];

                    // Agregar un filtro a ese rango
                    range.AutoFilter = true;
                    // Convertimos el paquete a un array de bytes
                    byte[] fileContents = package.GetAsByteArray();

                    // Devolvemos el archivo Excel como una descarga
                    return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "example.xlsx");
                }
            }
            else
            {
                return Content("Opcion no valida");
            }

        }
    }
    
}
