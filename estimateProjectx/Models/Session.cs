using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace estimateProject.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }
    }
}
