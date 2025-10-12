using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace NRL_PROJECT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public HealthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult CheckDatabase()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var cmd = new MySqlCommand("SELECT 1;", connection);
                cmd.ExecuteScalar();

                return Ok(new
                {
                    status = "Healthy",
                    database = "Connected",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    database = "Disconnected",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
