using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly NRL_Db_Context _context;
        private readonly IWebHostEnvironment _environment;

        public FileUploadController(NRL_Db_Context dbContext, IWebHostEnvironment environment)
        {
            _context = dbContext;
            _environment = environment;
        }

        // GET: viser opplastingsskjemaet
        [HttpGet]
        public IActionResult UploadReportImage(int obstacleId)
        {
            ViewBag.ObstacleId = obstacleId;
            return View();
        }

        // POST: håndterer innsending av fil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadReportImage(int obstacleId, FileUploadModel model)
        {
            if (ModelState.IsValid && model.File != null && model.File.Length > 0)
            {
                // 📁 Opprett mappe for opplastede filer hvis den ikke finnes
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 🧾 Generer et unikt filnavn
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 💾 Lagre filen fysisk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }


                // 📌 Opprett og lagre en ny ObstacleReportData‑rad
                var obstacleReport = new ObstacleReportData
                {
                    ObstacleID = obstacleId,                  // kobler til eksisterende hindring
                    ObstacleImageURL = "/uploads/" + uniqueFileName,
                    ObstacleReportDate = DateTime.UtcNow,
                    ObstacleReportStatus = ObstacleReportData.EnumTypes.New
                };

                _context.ObstacleReports.Add(obstacleReport);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Filen '{model.File.FileName}' ble lastet opp.";
                return RedirectToAction("Map", new { id = obstacleId });
            }

            ViewBag.Message = "Opplasting feilet. Kontroller at du har valgt en gyldig fil.";
            ViewBag.ObstacleId = obstacleId;
            return View(model);
        }
    }
}
