public class UserCourse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Platform { get; set; }
    public string Url { get; set; }
    public string Level { get; set; }
    public string Status { get; set; } = "Not Started";
    public string? Notes { get; set; }
    public int DurationHours { get; set; } // e.g., 3 (means 3 hours)


    public int UserId { get; set; }
}
