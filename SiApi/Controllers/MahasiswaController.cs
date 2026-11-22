
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.Models;

namespace SiApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MahasiswaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MahasiswaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Mahasiswa
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Mahasiswa
                .AsNoTracking()
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/Mahasiswa/{nim}
        [HttpGet("{nim}")]
        public async Task<IActionResult> GetByNim(string nim)
        {
            var data = await _context.Mahasiswa
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Nim == nim);

            if (data == null)
                return NotFound(new
                {
                    message = "Mahasiswa tidak ditemukan."
                });

            return Ok(data);
        }

        // POST: api/Mahasiswa
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] Mahasiswa model)
        {
            if (string.IsNullOrWhiteSpace(model.Nim) ||
                string.IsNullOrWhiteSpace(model.Nama))
                return BadRequest(new
                {
                    message = "NIM dan nama wajib diisi."
                });

            if (model.Nim.Length > 450 ||
                model.Nama.Length > 200)
                return BadRequest(new
                {
                    message = "Panjang NIM atau nama melebihi batas."
                });

            model.Nim = model.Nim.Trim();
            model.Nama = model.Nama.Trim();

            bool exists = await _context.Mahasiswa
                .AnyAsync(m => m.Nim == model.Nim);

            if (exists)
                return Conflict(new
                {
                    message = "NIM sudah terdaftar."
                });

            model.DaftarPresensi = null;

            try
            {
                _context.Mahasiswa.Add(model);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetByNim),
                    new { nim = model.Nim },
                    model
                );
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "Gagal menyimpan mahasiswa. Periksa kemungkinan NIM duplikat."
                });
            }
        }

        // PUT: api/Mahasiswa/{nim}
        [HttpPut("{nim}")]
        public async Task<IActionResult> Update(
            string nim, [FromBody] Mahasiswa model)
        {
            if (nim != model.Nim)
                return BadRequest(new
                {
                    message = "NIM tidak cocok."
                });

            if (string.IsNullOrWhiteSpace(model.Nama))
                return BadRequest(new
                {
                    message = "Nama wajib diisi."
                });

            model.Nama = model.Nama.Trim();

            if (model.Nama.Length > 200)
                return BadRequest(new
                {
                    message = "Nama terlalu panjang."
                });

            var existing = await _context.Mahasiswa.FindAsync(nim);

            if (existing == null)
                return NotFound(new
                {
                    message = "Mahasiswa tidak ditemukan."
                });

            existing.Nama = model.Nama;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Data mahasiswa berhasil diperbarui.",
                data = existing
            });
        }

        // DELETE: api/Mahasiswa/{nim}
        [HttpDelete("{nim}")]
        public async Task<IActionResult> Delete(string nim)
        {
            var mahasiswa = await _context.Mahasiswa.FindAsync(nim);

            if (mahasiswa == null)
                return NotFound(new
                {
                    message = "Mahasiswa tidak ditemukan."
                });

            bool hasPresensi = await _context.Presensi
                .AnyAsync(p => p.NimMahasiswa == nim);

            if (hasPresensi)
                return Conflict(new
                {
                    message = "Mahasiswa memiliki riwayat presensi dan tidak dapat dihapus."
                });

            _context.Mahasiswa.Remove(mahasiswa);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Mahasiswa berhasil dihapus."
            });
        }
    }
}
