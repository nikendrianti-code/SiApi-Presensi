using Microsoft.EntityFrameworkCore;
using SiApi.Data;
using SiApi.DTOs;
using SiApi.Models;
using System.Data;

namespace SiApi.Services;

public class PresensiService
{
    private readonly AppDbContext _db;

    // Koordinat Pusat Kampus & Batas Waktu 
    private const double LatKampus = -7.400894;
    private const double LonKampus = 109.231288;
    private const double RadiusMaksimalMeter = 500.0; 

    public PresensiService(AppDbContext db) => _db = db;

    public async Task<PresensiResponseDto> AbsenAsync(PresensiRequestDto req)
    {
        // Sinkronisasi: Database Transaction untuk mencegah Race Condition saat concurrent access
        using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            // 1. Cek validitas Mahasiswa & JadwalKuliah
            var mhsAda = await _db.Mahasiswa.AnyAsync(m => m.Nim == req.NimMahasiswa); 
            if (!mhsAda) return new(false, "NIM Mahasiswa tidak terdaftar", "Ditolak", 0, DateTime.Now);

            var jadwal = await _db.JadwalKuliah.FindAsync(req.JadwalKuliahId);
            if (jadwal == null) return new(false, "Jadwal kuliah tidak ditemukan", "Ditolak", 0, DateTime.Now);

            // 2. Validasi Waktu (Jam operasional presensi 07:00 - 21:00 WIB)
            var now = DateTime.UtcNow.AddHours(7);
            if(now.Hour < 7 || now.Hour > 23)
                return new(false, "Di luar jam presensi kuliah", "Ditolak", 0, now);

            // 3. Validasi Geolocation (Haversine Formula)
            double jarak = HitungJarak(req.Latitude, req.Longitude, LatKampus, LonKampus);
            if (jarak > RadiusMaksimalMeter)
                return new(false, $"Lokasi di luar radius kampus ({Math.Round(jarak, 1)}m)", "Ditolak", Math.Round(jarak, 2), now);

            // 4. Cek Duplikasi Presensi
            bool sudahAbsen = await _db.Presensi.AnyAsync(p =>
                p.NimMahasiswa == req.NimMahasiswa && p.JadwalKuliahId == req.JadwalKuliahId);

            if (sudahAbsen)
                return new(false, "Mahasiswa sudah melakukan presensi pada jadwal ini", "Ditolak", Math.Round(jarak, 2), now);

            // 5. Simpan ke Database
            var presensi = new Presensi
            {
                NimMahasiswa = req.NimMahasiswa,
                JadwalKuliahId = req.JadwalKuliahId,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                WaktuPresensi = now,
                Status = "Hadir",
                Keterangan = req.Keterangan ?? $"Jarak: {Math.Round(jarak, 1)}m"
            };

            _db.Presensi.Add(presensi);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return new(true, "Presensi berhasil dicatat", "Hadir", Math.Round(jarak, 2), now);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return new(false, "Gagal: Terdeteksi pengiriman ganda bersamaan (Race Condition)", "Ditolak", 0, DateTime.Now);
        }
    }

    private static double HitungJarak(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}