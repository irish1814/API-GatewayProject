using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json; // For JSON
using System.Text;
using System.Text.Json;     // For custom serialization

namespace ApiGateway.Controllers
{
    // http://localhost:5182/api/APIServices
    [Route("api/[controller]")]
    [ApiController]
    public class APIServices : Controller
    {
        [HttpGet("ip")]
        public async Task<IActionResult> GetResult()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://ipwho.is/9.9.9.9");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }

            return StatusCode((int)response.StatusCode, "Failed to fetch data.");
        }
    }
}
