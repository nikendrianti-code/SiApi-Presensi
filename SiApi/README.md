# SiApi-Presensi — Sistem Presensi Kuliah Geolocation & Waktu

Proyek Ujian Tengah Semester (UTS) Mata Kuliah **Sistem Terdistribusi**.
Studi Kasus No. 6: Presensi Kuliah Geolocation & Waktu.

## Anggota Kelompok
1. Niken Dwi Rianti (24SA21A043)
2. Tian Nurani (24SA21A041)

## Arsitektur & Mekanisme Sistem
- Arsitektur: Client-Server (RESTful Web API menggunakan ASP.NET Core & Entity Framework Core).
- Komunikasi Antarproses (IPC): HTTP/HTTPS dengan pertukaran payload berformat JSON (`PresensiRequestDto` & `PresensiResponseDto`).
- Validasi Geolocation: Rumus Haversine untuk menghitung jarak koordinat GPS mahasiswa terhadap koordinat kampus.
- Sinkronisasi Data & Concurrency: Menggunakan `Database Transaction` (`IsolationLevel.Serializable`) dan `Unique Composite Index` pada `(NimMahasiswa, JadwalKuliahId)` untuk mencegah *race condition* saat akses bersamaan.

## Cara Menjalankan Proyek
1. Sesuaikan *Connection String* database pada `appsettings.json`.
2. Jalankan migrasi database:
   ```bash
   dotnet ef database update