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
    public class APIServicesController : Controller
    {
        [HttpPost("agent")]
        public async Task<IActionResult> AskLAIAgent([FromForm] string prompt)
        {
            var payload = new
            {
                prompt = prompt,
                stream = false
            };

            var jsonPayload = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var client = new HttpClient();
            var response = await client.PostAsync("http://localhost:11434/api/generate", jsonPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }

            var llamaResponse = await response.Content.ReadAsStringAsync();

            return Ok(llamaResponse);
        }

        /*
         * Get info of a crypto currency using its ID from the API Service - coinlore
         */
        [HttpPost("Currency")]
        public async Task<IActionResult> GetCryptoCurrencyInfo([FromForm]int id)
        {
            var client = new HttpClient();
            var response = await client.GetAsync("https://api.coinlore.net/api/ticker/?id=" + id);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Ok(content);
            }

            return StatusCode((int)response.StatusCode, "Failed to fetch data");
        }
    }
}
