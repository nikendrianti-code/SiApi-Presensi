using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.Models;
using SiApi.Data;
using SiApi.Models;
using System;
using System.Threading.Tasks;

namespace SiatApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresensiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PresensiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPresensi([FromBody] Presensi request)
        {
            // 1. Membuka transaksi untuk mengunci proses (mencegah race condition)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Cek apakah NIM tersebut sudah absen pada Jadwal yang sama
                bool isSudahAbsen = await _context.Presensi
                    .AnyAsync(p => p.NimMahasiswa == request.NimMahasiswa &&
                                   p.JadwalKuliahId == request.JadwalKuliahId);

                if (isSudahAbsen)
                {
                    return Conflict(new { message = "Mahasiswa sudah melakukan presensi." });
                }

                // 3. Validasi Geolocation (Toleransi radius koordinat kampus)
                double kampusLat = -7.4245; // Contoh latitude kampus
                double kampusLon = 109.2291; // Contoh longitude kampus

                bool isWithinRadius = (Math.Abs(request.Latitude - kampusLat) < 0.01) &&
                                      (Math.Abs(request.Longitude - kampusLon) < 0.01);

                if (!isWithinRadius)
                {
                    return BadRequest(new { message = "Lokasi berada di luar area kampus." });
                }

                // 4. Set waktu dari server dan simpan data
                request.WaktuPresensi = DateTime.UtcNow;
                request.Status = "Hadir";

                _context.Presensi.Add(request);
                await _context.SaveChangesAsync();

                // 5. Commit transaksi jika semua lolos
                await transaction.CommitAsync();

                return Ok(new { message = "Presensi berhasil dicatat.", data = request });
            }
            catch (Exception ex)
            {
                // Rollback jika terjadi kesalahan server saat akses bersamaan
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Terjadi kesalahan server.", error = ex.Message });
            }
        }
    }
}