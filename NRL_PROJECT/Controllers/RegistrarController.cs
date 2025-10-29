using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Data;
using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

namespace NRL_PROJECT.Controllers
    {
        public class RegistrarController : Controller
        {
            private readonly NRL_Db_Context _context;

            public RegistrarController(NRL_Db_Context context)
            {
                _context = context;
            }

            // Henter alle rapporter
            public async Task<IActionResult> RegistrarView()
            {
                var reports = await _context.ObstacleReports
                    .Include(r => r.Obstacle)  // kobler til hinderdetaljer
                    .OrderByDescending(r => r.Time_of_Submitted_Report)
                    .ToListAsync();

                return View(reports);
            }

            // Viser detaljer om én rapport
            public async Task<IActionResult> ReportDetails(int id)
            {
                var report = await _context.ObstacleReports
                    .Include(r => r.Obstacle)
                    .FirstOrDefaultAsync(r => r.Report_Id == id);

                if (report == null)
                    return NotFound();

                return View(report);
            }
        }
    }
