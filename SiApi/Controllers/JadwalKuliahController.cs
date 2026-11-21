
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.Models;

namespace SiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JadwalKuliahController : ControllerBase
    {
        private readonly AppDbContext _context;

        public JadwalKuliahController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/JadwalKuliah
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.JadwalKuliah.ToListAsync();
            return Ok(data);
        }

        // GET: api/JadwalKuliah/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _context.JadwalKuliah
                .FindAsync(id);

            if (data == null)
                return NotFound(new { message = "Jadwal kuliah tidak ditemukan." });

            return Ok(data);
        }

        // POST: api/JadwalKuliah
        [HttpPost]
        public async Task<IActionResult> Create(JadwalKuliah model)
        {
            if (string.IsNullOrWhiteSpace(model.NamaMataKuliah))
                return BadRequest(new { message = "Nama mata kuliah wajib diisi." });

            model.Id = 0;
            model.DaftarPresensi = null;

            _context.JadwalKuliah.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = model.Id },
                model
            );
        }

        // PUT: api/JadwalKuliah/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, JadwalKuliah model)
        {
            if (id != model.Id)
                return BadRequest(new { message = "ID jadwal tidak cocok." });

            var existing = await _context.JadwalKuliah.FindAsync(id);

            if (existing == null)
                return NotFound(new { message = "Jadwal kuliah tidak ditemukan." });

            if (string.IsNullOrWhiteSpace(model.NamaMataKuliah))
                return BadRequest(new { message = "Nama mata kuliah wajib diisi." });

            existing.NamaMataKuliah = model.NamaMataKuliah;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Jadwal kuliah berhasil diperbarui.",
                data = existing
            });
        }

        // DELETE: api/JadwalKuliah/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var jadwal = await _context.JadwalKuliah.FindAsync(id);

            if (jadwal == null)
                return NotFound(new { message = "Jadwal kuliah tidak ditemukan." });

            bool hasPresensi = await _context.Presensi
                .AnyAsync(p => p.JadwalKuliahId == id);

            if (hasPresensi)
                return Conflict(new
                {
                    message = "Jadwal memiliki riwayat presensi dan tidak dapat dihapus."
                });

            _context.JadwalKuliah.Remove(jadwal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Jadwal kuliah berhasil dihapus." });
        }
    }
}
