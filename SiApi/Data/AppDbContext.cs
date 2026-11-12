using Microsoft.EntityFrameworkCore;
using SiApi.Models;

namespace SiApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Presensi> Presensi { get; set; }
        public DbSet<Mahasiswa> Mahasiswa { get; set; } // Tabel Master baru
        public DbSet<JadwalKuliah> JadwalKuliah { get; set; } // Tabel Master baru
    }
}