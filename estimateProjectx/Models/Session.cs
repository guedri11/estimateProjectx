using Microsoft.AspNetCore.Identity;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace estimateProjectx.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }
        public int VotesCount { get; set; } = 0;
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();

        public DateTime CreatedAt { get; set; }
    }
}
