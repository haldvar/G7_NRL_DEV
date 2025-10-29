using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRL_PROJECT.Data;
using NRL_PROJECT.Models;
using System.Text.Json;

namespace NRL_PROJECT.Controllers
{
    public class ReportController : Controller
    {
        private readonly NRL_Db_Context _context;

        public ReportController(NRL_Db_Context context)
        {
            _context = context;
        }

      
    }
}
