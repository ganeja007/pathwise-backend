using Microsoft.EntityFrameworkCore;
using PathwiseAPI.Models;

namespace PathwiseAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        public DbSet<UserCourse> UserCourses { get; set; }

    }
}
