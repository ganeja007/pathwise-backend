using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PathwiseAPI.Models
{
    public class UserInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Value { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
