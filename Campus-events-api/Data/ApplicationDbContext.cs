using Campus_events_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Campus_events_api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
    }
}