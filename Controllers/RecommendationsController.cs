using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PathwiseAPI.Data;
using PathwiseAPI.Models;
using System.Text.Json;
using System.Net.Http.Headers;

namespace PathwiseAPI.Controllers
{
    [Route("api/recommendations")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public RecommendationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRecommendations(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var prompt = $"""
Suggest two online courses (from Coursera, Udemy, or edX) and one course from California State University, Fullerton (CSUF) for a student majoring in {user.Major} with a career goal of {user.CareerGoal}.

Return a JSON array of 3 course objects with the following fields:
- title (string)
- description (string)
- platform (string)
- instructor (string)
- duration (string)
- level (string)
- tags (array of strings)
- url (string, if available — otherwise leave it empty)

Do not include markdown formatting or extra text — return only valid JSON.
""";


            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] {
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
            var rawContent = doc.RootElement.GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
            // Remove markdown code block formatting: ```json\n...\n```
var cleanedContent = rawContent?
    .Replace("```json", "")
    .Replace("```", "")
    .Trim();

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

  
                var recommendedCourses = JsonSerializer.Deserialize<List<Course>>(cleanedContent!, options);

                
               foreach (var course in recommendedCourses)
               {           
                    if (string.IsNullOrWhiteSpace(course.Title))
                        course.Title = $"{course.Platform} Course";

                    if (string.IsNullOrWhiteSpace(course.Url))
                        course.Url = $"https://www.google.com/search?q={Uri.EscapeDataString(course.Title)}";
               }     
                return Ok(recommendedCourses);
            }
            catch
            {
                return StatusCode(500, new { message = "Failed to parse course recommendations." });
            }
        }
    }
}
