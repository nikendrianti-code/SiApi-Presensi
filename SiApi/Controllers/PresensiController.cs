using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.DTOs;
using SiApi.Models;
using SiApi.Services;

namespace SiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresensiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PresensiService _presensiService;

        private static readonly string[] StatusValid =
        {
            "Hadir", "Izin", "Sakit", "Alpha"
        };

        public PresensiController(AppDbContext context, PresensiService presensiService)
        {
            _context = context;
            _presensiService = presensiService;
        }

        // POST: api/Presensi/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitPresensi([FromBody] PresensiRequestDto dto)
        {
            var result = await _presensiService.SubmitPresensiAsync(dto);

            if (!result.Sukses)
            {
                if (result.JarakMeter.HasValue)
                {
                    return StatusCode(result.StatusCode, new
                    {
                        message = result.Pesan,
                        jarakMeter = result.JarakMeter.Value
                    });
                }

                return StatusCode(result.StatusCode, new
                {
                    message = result.Pesan
                });
            }

            return Ok(new
            {
                message = result.Pesan,
                data = result.Data
            });
        }

        // GET: api/Presensi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Presensi>>> GetAllPresensi()
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
        public async Task<ActionResult<Presensi>> GetPresensiById(int id)
        {
            var data = await _context.Presensi
                .AsNoTracking()
                .Include(p => p.Mahasiswa)
                .Include(p => p.JadwalKuliah)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (data == null)
            {
                return NotFound(new { message = "Data presensi tidak ditemukan." });
            }

            return Ok(data);
        }

        // PUT: api/Presensi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePresensi(int id, [FromBody] Presensi model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "ID presensi tidak cocok." });

            var existing = await _context.Presensi.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Data presensi tidak ditemukan." });

            if (string.IsNullOrWhiteSpace(model.Status))
                return BadRequest(new { message = "Status presensi wajib diisi." });

            string? status = StatusValid.FirstOrDefault(s =>
                s.Equals(model.Status.Trim(), StringComparison.OrdinalIgnoreCase));

            if (status == null)
                return BadRequest(new { message = "Status harus Hadir, Izin, Sakit, atau Alpha." });

            if (model.Keterangan?.Length > 500)
                return BadRequest(new { message = "Keterangan maksimal 500 karakter." });

            existing.Status = status;
            existing.Keterangan = model.Keterangan?.Trim();

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Data presensi berhasil diperbarui.", data = existing });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Gagal memperbarui data presensi." });
            }
        }

        // DELETE: api/Presensi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresensi(int id)
        {
            var data = await _context.Presensi.FindAsync(id);
            if (data == null)
                return NotFound(new { message = "Data presensi tidak ditemukan." });

            _context.Presensi.Remove(data);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Data presensi berhasil dihapus." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Gagal menghapus data presensi." });
            }
        }
    }
}