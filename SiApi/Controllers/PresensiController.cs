
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
        private readonly AppDbContext _context;

        // Koordinat kampus
        private const double TargetLat = -7.4245;
        private const double TargetLon = 109.2291;
        private const double MaxRadiusKm = 0.5;

        // Status yang diizinkan
        private static readonly string[] StatusValid =
        {
            "Hadir", "Izin", "Sakit", "Alpha"
        };

        public PresensiController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Presensi/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPresensi(
            [FromBody] Presensi model)
        {
            // Validasi koordinat
            if (!double.IsFinite(model.Latitude) ||
                !double.IsFinite(model.Longitude) ||
                model.Latitude < -90 || model.Latitude > 90 ||
                model.Longitude < -180 || model.Longitude > 180)
            {
                return BadRequest(new
                {
                    message = "Koordinat lokasi tidak valid."
                });
            }

            // Validasi NIM
            if (string.IsNullOrWhiteSpace(model.NimMahasiswa))
            {
                return BadRequest(new
                {
                    message = "NIM mahasiswa wajib diisi."
                });
            }

            model.NimMahasiswa = model.NimMahasiswa.Trim();

            // Validasi jadwal
            if (model.JadwalKuliahId <= 0)
            {
                return BadRequest(new
                {
                    message = "Jadwal kuliah wajib dipilih."
                });
            }

            // Validasi keterangan
            if (model.Keterangan?.Length > 500)
            {
                return BadRequest(new
                {
                    message = "Keterangan maksimal 500 karakter."
                });
            }

            // Memastikan mahasiswa terdaftar
            bool mahasiswaAda = await _context.Mahasiswa
                .AnyAsync(m => m.Nim == model.NimMahasiswa);

            if (!mahasiswaAda)
            {
                return NotFound(new
                {
                    message = "Mahasiswa tidak ditemukan."
                });
            }

            // Memastikan jadwal terdaftar
            bool jadwalAda = await _context.JadwalKuliah
                .AnyAsync(j => j.Id == model.JadwalKuliahId);

            if (!jadwalAda)
            {
                return NotFound(new
                {
                    message = "Jadwal kuliah tidak ditemukan."
                });
            }

            // Menghitung jarak ke kampus
            double distance = CalculateDistance(
                TargetLat,
                TargetLon,
                model.Latitude,
                model.Longitude
            );

            if (distance > MaxRadiusKm)
            {
                return BadRequest(new
                {
                    message = "Anda berada di luar radius presensi 500 meter.",
                    jarakMeter = Math.Round(distance * 1000, 2)
                });
            }

            // Pemeriksaan awal presensi ganda
            bool alreadyExists = await _context.Presensi
                .AnyAsync(p =>
                    p.NimMahasiswa == model.NimMahasiswa &&
                    p.JadwalKuliahId == model.JadwalKuliahId);

            if (alreadyExists)
            {
                return Conflict(new
                {
                    message = "Mahasiswa sudah melakukan presensi pada jadwal ini."
                });
            }

            // Data penting ditentukan oleh server
            model.Id = 0;
            model.WaktuPresensi = DateTime.UtcNow;
            model.Status = "Hadir";

            // Mencegah client mengirim objek relasi
            model.Mahasiswa = null;
            model.JadwalKuliah = null;

            try
            {
                _context.Presensi.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Presensi berhasil dicatat.",
                    data = model
                });
            }
            catch (DbUpdateException)
            {
                // Menangani race condition:
                // dua request bisa lolos pemeriksaan awal,
                // tetapi unique index database mencegah duplikasi.
                return Conflict(new
                {
                    message = "Presensi gagal. Data mungkin sudah tercatat."
                });
            }
        }

        // GET: api/Presensi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Presensi>>>
            GetAllPresensi()
        {
            var data = await _context.Presensi
                .AsNoTracking()
                .Include(p => p.Mahasiswa)
                .Include(p => p.JadwalKuliah)
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/Presensi/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Presensi>>
            GetPresensiById(int id)
        {
            var data = await _context.Presensi
                .AsNoTracking()
                .Include(p => p.Mahasiswa)
                .Include(p => p.JadwalKuliah)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Data presensi tidak ditemukan."
                });
            }

            return Ok(data);
        }

        // PUT: api/Presensi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePresensi(
            int id, [FromBody] Presensi model)
        {
            if (id != model.Id)
            {
                return BadRequest(new
                {
                    message = "ID presensi tidak cocok."
                });
            }

            var existing = await _context.Presensi.FindAsync(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    message = "Data presensi tidak ditemukan."
                });
            }

            if (string.IsNullOrWhiteSpace(model.Status))
            {
                return BadRequest(new
                {
                    message = "Status presensi wajib diisi."
                });
            }

            // Validasi status
            string? status = StatusValid.FirstOrDefault(s =>
                s.Equals(model.Status.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (status == null)
            {
                return BadRequest(new
                {
                    message = "Status harus Hadir, Izin, Sakit, atau Alpha."
                });
            }

            if (model.Keterangan?.Length > 500)
            {
                return BadRequest(new
                {
                    message = "Keterangan maksimal 500 karakter."
                });
            }

            // Hanya status dan keterangan yang boleh diubah
            existing.Status = status;
            existing.Keterangan = model.Keterangan?.Trim();

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Data presensi berhasil diperbarui.",
                    data = existing
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    message = "Gagal memperbarui data presensi."
                });
            }
        }

        // DELETE: api/Presensi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresensi(int id)
        {
            var data = await _context.Presensi.FindAsync(id);

            if (data == null)
            {
                return NotFound(new
                {
                    message = "Data presensi tidak ditemukan."
                });
            }

            _context.Presensi.Remove(data);

            try
            {
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Data presensi berhasil dihapus."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new
                {
                    message = "Gagal menghapus data presensi."
                });
            }
        }

        // Menghitung jarak menggunakan Haversine
        private double CalculateDistance(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a =
                Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(ToRadians(lat1)) *
                Math.Cos(ToRadians(lat2)) *
                Math.Pow(Math.Sin(dLon / 2), 2);

            // Menjaga nilai a tetap pada rentang valid
            a = Math.Clamp(a, 0.0, 1.0);

            double c = 2 * Math.Atan2(
                Math.Sqrt(a),
                Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private double ToRadians(double angle)
        {
            return angle * Math.PI / 180;
        }
    }
}
