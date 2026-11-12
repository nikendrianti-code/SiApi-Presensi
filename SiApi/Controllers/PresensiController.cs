using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.Models;

namespace SiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresensiController : ControllerBase
    {
        readonly AppDbContext _context;

        const double TargetLat = -7.4245;
        const double TargetLon = 109.2291;
        const double MaxRadiusKm = 0.5; // Radius 500 meter

        public PresensiController(AppDbContext context)
        {
            _context = context;
        }

        // 1. CREATE (Submit Presensi dengan Validasi Radius & Duplikasi)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPresensi([FromBody] Presensi model)
        {
            double distance = CalculateDistance(TargetLat, TargetLon, model.Latitude, model.Longitude);
            if (distance > MaxRadiusKm)
            {
                return BadRequest(new { message = "Gagal: Anda berada di luar radius lokasi presensi yang diizinkan." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool alreadyExists = await _context.Presensi
                    .AnyAsync(p => p.NimMahasiswa == model.NimMahasiswa && p.JadwalKuliahId == model.JadwalKuliahId);

                if (alreadyExists)
                {
                    return Conflict(new { message = "Gagal: Mahasiswa sudah melakukan presensi pada jadwal ini." });
                }

                model.WaktuPresensi = DateTime.Now;
                _context.Presensi.Add(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Presensi berhasil dicatat.", data = model });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Terjadi kesalahan pada server.", error = ex.Message });
            }
        }

        // 2. READ: Mendapatkan seluruh daftar riwayat presensi (Beserta data relasinya)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Presensi>>> GetAllPresensi()
        {
            var list = await _context.Presensi
                .Include(p => p.Mahasiswa)
                .Include(p => p.JadwalKuliah)
                .ToListAsync();
            return Ok(list);
        }

        // 3. READ: Mendapatkan presensi berdasarkan ID tertentu
        [HttpGet("{id}")]
        public async Task<ActionResult<Presensi>> GetPresensiById(int id)
        {
            var presensi = await _context.Presensi
                .Include(p => p.Mahasiswa)
                .Include(p => p.JadwalKuliah)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (presensi == null)
            {
                return NotFound(new { message = "Data presensi tidak ditemukan." });
            }
            return Ok(presensi);
        }

        // 4. UPDATE: Mengubah data presensi (Status & Keterangan)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePresensi(int id, [FromBody] Presensi model)
        {
            if (id != model.Id)
            {
                return BadRequest(new { message = "ID Presensi tidak cocok." });
            }

            var existingPresensi = await _context.Presensi.FindAsync(id);
            if (existingPresensi == null)
            {
                return NotFound(new { message = "Data presensi yang akan diubah tidak ditemukan." });
            }

            existingPresensi.Status = model.Status;
            existingPresensi.Keterangan = model.Keterangan;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Data presensi berhasil diperbarui.", data = existingPresensi });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Gagal memperbarui data.", error = ex.Message });
            }
        }

        // 5. DELETE: Menghapus data presensi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresensi(int id)
        {
            var presensi = await _context.Presensi.FindAsync(id);
            if (presensi == null)
            {
                return NotFound(new { message = "Data presensi tidak ditemukan." });
            }

            _context.Presensi.Remove(presensi);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data presensi berhasil dihapus." });
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return 6371 * c;
        }

        private double ToRadians(double angle) => angle * Math.PI / 180;
    }
}