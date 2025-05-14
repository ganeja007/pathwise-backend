using System.ComponentModel.DataAnnotations;

namespace PathwiseAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Major { get; set; } = null!;
        public string AcademicYear { get; set; } = null!;
        public string CareerGoal { get; set; } = null!;

        public List<UserInterest> Interests { get; set; } = new();
    }
}
