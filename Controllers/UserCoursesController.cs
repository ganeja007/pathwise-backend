using Microsoft.AspNetCore.Mvc;
using PathwiseAPI.Data; // adjust namespace
using PathwiseAPI.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class UserCoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserCoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddUserCourse([FromBody] UserCourse course)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.UserCourses.Add(course);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Course saved successfully!" });
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetCoursesForUser(int userId)
    {
        var courses = await _context.UserCourses
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return Ok(courses);
    }

    [HttpPut("{id}/complete")]
    public async Task<IActionResult> MarkCourseAsCompleted(int id)
{
    var course = await _context.UserCourses.FindAsync(id);
    if (course == null)
        return NotFound(new { message = "Course not found" });

    course.Status = "Completed";
    await _context.SaveChangesAsync();

    return Ok(new { message = "Course marked as completed" });
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUserCourse(int id)
{
    var course = await _context.UserCourses.FindAsync(id);
    if (course == null)
        return NotFound(new { message = "Course not found" });

    _context.UserCourses.Remove(course);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Course deleted successfully" });
}


}
