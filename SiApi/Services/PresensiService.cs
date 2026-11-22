using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.DTOs;
using SiApi.Models;
using System.Data;

namespace SiApi.Services
{
    public class PresensiService
    {
        private readonly AppDbContext _context;

        // Koordinat kampus & radius disesuaikan dengan PresensiController
        private const double TargetLat = -7.4245;
        private const double TargetLon = 109.2291;
        private const double MaxRadiusKm = 0.5; // 500 meter

        public PresensiService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Sukses, int StatusCode, string Pesan, object? Data, double? JarakMeter)> SubmitPresensiAsync(PresensiRequestDto dto)
        {
            // 1. Validasi koordinat
            if (!double.IsFinite(dto.Latitude) ||
                !double.IsFinite(dto.Longitude) ||
                dto.Latitude < -90 || dto.Latitude > 90 ||
                dto.Longitude < -180 || dto.Longitude > 180)
            {
                return (false, 400, "Koordinat lokasi tidak valid.", null, null);
            }

            // 2. Validasi NIM & Jadwal & Keterangan
            if (string.IsNullOrWhiteSpace(dto.NimMahasiswa))
                return (false, 400, "NIM mahasiswa wajib diisi.", null, null);

            string nim = dto.NimMahasiswa.Trim();

            if (dto.JadwalKuliahId <= 0)
                return (false, 400, "Jadwal kuliah wajib dipilih.", null, null);

            if (dto.Keterangan?.Length > 500)
                return (false, 400, "Keterangan maksimal 500 karakter.", null, null);

            // 3. Validasi keberadaan Mahasiswa & Jadwal
            bool mahasiswaAda = await _context.Mahasiswa.AnyAsync(m => m.Nim == nim);
            if (!mahasiswaAda)
                return (false, 404, "Mahasiswa tidak ditemukan.", null, null);

            bool jadwalAda = await _context.JadwalKuliah.AnyAsync(j => j.Id == dto.JadwalKuliahId);
            if (!jadwalAda)
                return (false, 404, "Jadwal kuliah tidak ditemukan.", null, null);

            // 4. Validasi radius Haversine (500 meter)
            double distanceKm = CalculateDistance(TargetLat, TargetLon, dto.Latitude, dto.Longitude);
            if (distanceKm > MaxRadiusKm)
            {
                return (false, 400, "Anda berada di luar radius presensi 500 meter.", null, Math.Round(distanceKm * 1000, 2));
            }

            // 5. Transaksi Serializable untuk mencegah Race Condition
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                bool alreadyExists = await _context.Presensi.AnyAsync(p =>
                    p.NimMahasiswa == nim &&
                    p.JadwalKuliahId == dto.JadwalKuliahId);

                if (alreadyExists)
                {
                    await transaction.RollbackAsync();
                    return (false, 409, "Mahasiswa sudah melakukan presensi pada jadwal ini.", null, null);
                }

                var presensiBaru = new Presensi
                {
                    NimMahasiswa = nim,
                    JadwalKuliahId = dto.JadwalKuliahId,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    WaktuPresensi = DateTime.UtcNow,
                    Status = "Hadir",
                    Keterangan = dto.Keterangan?.Trim()
                };

                _context.Presensi.Add(presensiBaru);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var hasil = await _context.Presensi
                    .AsNoTracking()
                    .Include(p => p.Mahasiswa)
                    .Include(p => p.JadwalKuliah)
                    .FirstOrDefaultAsync(p => p.Id == presensiBaru.Id);

                return (true, 200, "Presensi berhasil dicatat.", hasil, null);
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return (false, 409, "Presensi gagal. Data sudah tercatat oleh transaksi lain.", null, null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, 409, "Terjadi bentrokan transaksi konkurensi. Presensi dibatalkan.", null, null);
            }
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371;
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Pow(Math.Sin(dLon / 2), 2);

            a = Math.Clamp(a, 0.0, 1.0);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private double ToRadians(double angle) => angle * Math.PI / 180;
    }
}