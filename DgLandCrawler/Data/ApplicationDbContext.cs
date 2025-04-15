using DgLandCrawler.Models;
using Microsoft.EntityFrameworkCore;

namespace DgLandCrawler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContext) : base(dbContext) { }

        public virtual DbSet<DGProductData> DGProducts { get; set; }
        public virtual DbSet<GoogleSearchResult> GoogleSearchResults { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
