using Microsoft.AspNetCore.Mvc;
using NRL_PROJECT.Models;

namespace NRL_PROJECT.Controllers
{
    public class FileUploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public FileUploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Viser opplastingsskjemaet
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Håndterer innsending av fil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid && model.File != null && model.File.Length > 0)
            {
                // 📁 Opprett mappe for opplastede filer hvis den ikke finnes
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // 🧾 Generer et unikt filnavn for å unngå duplikater
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 💾 Lagre filen fysisk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                // Vis bekreftelse til bruker
                ViewBag.Message = $"Filen '{model.File.FileName}' ble lastet opp som '{uniqueFileName}'.";
                ViewBag.FilePath = "/uploads/" + uniqueFileName;

                return View();
            }

            ViewBag.Message = "Opplasting feilet. Kontroller at du har valgt en gyldig fil.";
            return View(model);
        }
    }
}
