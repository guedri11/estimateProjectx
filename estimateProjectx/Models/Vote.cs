using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace estimateProjectx.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }
    }
}
