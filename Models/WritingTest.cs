using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FastFingerTest.Models
{
    public class WritingTest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Length { get; set; }
        public int MaxPoints { get; set; }
        public DateTime CreationTime { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
    }
}
