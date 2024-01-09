using estimateProjectx.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using estimateProjectx.Models;

namespace estimateProjectx.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<estimateProjectx.Models.Vote>? Vote { get; set; }
    }
}