using Microsoft.EntityFrameworkCore;
using SiApi.Models;
using SiApi.Models;

namespace SiApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Presensi> Presensi { get; set; }
    }
}