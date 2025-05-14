public class Course
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Platform { get; set; }
    public string Instructor { get; set; }
    public string Duration { get; set; }
    public string Level { get; set; }
    public string Match { get; set; } = "95%";
    public List<string> Tags { get; set; } = new();
    public string Url { get; set; }

    // Optional UI enhancements
    public string Image { get; set; } = "/placeholder.svg";
    public bool Recommended { get; set; } = true;
    public bool Popular { get; set; } = false;
    public bool New { get; set; } = false;
}
