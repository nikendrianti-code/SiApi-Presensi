using Microsoft.EntityFrameworkCore;
using SiApi.Models;

namespace SiApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Presensi> Presensi { get; set; }
        public DbSet<Mahasiswa> Mahasiswa { get; set; } 
        public DbSet<JadwalKuliah> JadwalKuliah { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Presensi>()
                .HasIndex(p => new
                {
                    p.NimMahasiswa,
                    p.JadwalKuliahId
                })
                .IsUnique();
        }
    }
}