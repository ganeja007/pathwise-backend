// ✅ Finalized CoursesController.cs forcing local logo images only

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathwiseAPI.Data;
using PathwiseAPI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PathwiseAPI.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CoursesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpGet("recommend")]
        public async Task<IActionResult> RecommendCourses([FromQuery] int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var prompt = $"""
Suggest 6 total courses:
- 2 Coursera
- 2 Udemy
- 2 CSUF

Tailor to a student majoring in {user.Major} with a career goal of {user.CareerGoal}.

Return a JSON array of course objects with:
- title
- description
- platform
- instructor
- duration
- level
- tags (array)
- url
- image (a relevant course image URL; for CSUF use: \"https://www.fullerton.edu/_resources/images/logo.svg\")

Return only valid raw JSON — no markdown.
""";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful academic course recommender." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return StatusCode(500, new { message = "OpenAI API key not configured." });

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await _httpClient.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = "OpenAI API error", error });
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resultJson);
            var rawContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var cleaned = rawContent?
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var courses = JsonSerializer.Deserialize<List<Course>>(cleaned!, options);

                foreach (var c in courses)
                {
                    // Force all images to local logos only
                    c.Image = c.Platform.ToLower() switch
                    {
                        "coursera" => "/logos/coursera.png",
                        "udemy" => "/logos/udemy.png",
                        "csuf" or "csuf curriculum" => "/logos/csuf.png",
                        _ => "/placeholder.svg"
                    };

                    if (string.IsNullOrWhiteSpace(c.Url))
                    {
                        c.Url = $"https://www.google.com/search?q={Uri.EscapeDataString(c.Title)}";
                    }

                    c.Recommended = true;
                    c.Match = "95%";
                }

                return Ok(new { courses });
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to parse response from OpenAI." });
            }
        }
    }
}
