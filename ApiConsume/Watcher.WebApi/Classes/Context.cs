using Microsoft.EntityFrameworkCore;

namespace Watcher.WebApi.Classes
{
    public class Context:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=AS-TASKIN63;Database=Watcher;User Id=sa;Password=1q2w3e4r+!;TrustServerCertificate=True");
        }

        public DbSet<Admin> Admins { get; set; }
    }
}
