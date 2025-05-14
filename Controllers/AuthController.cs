using Microsoft.AspNetCore.Mvc;
using PathwiseAPI.Models;
using PathwiseAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PathwiseAPI.Services;

namespace PathwiseAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AuthController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid user data", errors = ModelState });

            if (user == null)
                return BadRequest(new { message = "Invalid request" });

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User already exists" });

            try
            {
                user.Interests = null;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var mockToken = Guid.NewGuid().ToString();
                return new JsonResult(new
                {
                    message = "User registered successfully",
                    data = new { token = mockToken, userId = user.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Interests)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid user data", errors = ModelState });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Major = updatedUser.Major;
            user.AcademicYear = updatedUser.AcademicYear;
            user.CareerGoal = updatedUser.CareerGoal;

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update user", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.Password != request.Password)
                return Unauthorized(new { message = "Invalid email or password" });

            var mockToken = Guid.NewGuid().ToString();
            return Ok(new
            {
                message = "Login successful",
                data = new { token = mockToken, userId = user.Id }
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Ok(new { message = "If an account exists, we’ve sent instructions." });

            var token = Guid.NewGuid().ToString();
            var resetLink = $"http://localhost:3000/reset-password?email={request.Email}";

            await _emailService.SendPasswordResetEmail(request.Email, resetLink);

            return Ok(new { message = "Reset email sent if account exists." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Email and new password are required." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest(new { message = "User not found." });

            user.Password = request.NewPassword; // No hashing for demo
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password reset successfully." });
        }
    }
}
