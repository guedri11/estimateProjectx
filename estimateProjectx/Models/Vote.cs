using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace estimateProjectx.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int VoteValue { get; set; }

        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public Session? Session { get; set; }
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }
    }
}
