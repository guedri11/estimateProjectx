using MessagePack;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace estimateProjectx.Models
{
    public class Vote
    {
        public int Id { get; set; }
        [Remote(action: "VerifyUniqueVote", controller: "Votes", AdditionalFields = "SessionId", ErrorMessage = "You have already voted for this session.")]
        public int VoteValue { get; set; }
        [Required]
        public int? SessionId { get; set; }
        [ForeignKey("SessionId")]
        public Session? Session { get; set; }
        [Required]
        public string? IdentityUserId { get; set; }
        [ForeignKey("IdentityUserId")]
        public IdentityUser? User { get; set; }
    }
}
