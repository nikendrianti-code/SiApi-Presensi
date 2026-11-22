# SiApi-Presensi — Sistem Presensi Mahasiswa Berbasis Lokasi & Transaksi Konkurensi

RESTful Web API berbasis **ASP.NET Core** dan **Entity Framework Core** untuk pencatatan presensi perkuliahan di **Universitas Amikom Purwokerto**. Sistem ini menerapkan validasi radius geografis menggunakan rumus **Haversine** serta pencegahan *race condition* melalui transaksi database **`IsolationLevel.Serializable`** dan *Unique Composite Index*.

---

## Fitur Utama

1. **Validasi Lokasi GPS (Haversine Formula)**
   - Menghitung jarak koordinat mahasiswa terhadap titik pusat kampus Universitas Amikom Purwokerto (`Latitude: -7.4245`, `Longitude: 109.2291`).
   - Menolak pengajuan presensi apabila posisi berada di luar radius maksimal **500 meter** (`400 Bad Request`).
2. **Pencegahan Duplikasi & Race Condition**
   - Mengamankan proses pencatatan menggunakan `IsolationLevel.Serializable` pada `PresensiService` dan *Unique Index* pada kombinasi `(NimMahasiswa, JadwalKuliahId)`.
   - Menjamin tidak terjadi pencatatan ganda meskipun puluhan *request* dikirimkan secara bersamaan dalam milidetik yang sama (`409 Conflict`).
3. **Pemisahan Arsitektur (Clean Separation)**
   - **Models:** Entitas relasional `Mahasiswa`, `JadwalKuliah`, dan `Presensi`.
   - **DTOs:** `PresensiRequestDto` untuk membatasi input klien agar `WaktuPresensi` dan `Status` dikendalikan penuh oleh server.
   - **Services:** `PresensiService` untuk menangani aturan bisnis, perhitungan Haversine, dan transaksi database.
   - **Controllers:** Endpoint REST API untuk manajemen data master dan transaksi presensi.

---

## Struktur Proyek

```text
SiApi/
├── Controllers/
│   ├── JadwalKuliahController.cs
│   ├── MahasiswaController.cs
│   └── PresensiController.cs
├── Data/
│   └── AppDbContext.cs
├── DTOs/
│   └── PresensiRequestDto.cs
├── Migrations/
├── Models/
│   ├── JadwalKuliah.cs
│   ├── Mahasiswa.cs
│   └── Presensi.cs
├── Services/
│   └── PresensiService.cs
├── Tests/
│   └── test_race.ps1
├── appsettings.json
└── Program.cs

Cara Menjalankan 
1. ProyekClone RepositoriBashgit clone
   [https://github.com/nikendrianti-code/SiApi-Presensi.git](https://github.com/nikendrianti-code/SiApi-Presensi.git)
   cd SiApi-Presensi/SiApi
2. Migrasi Database (SQL Server LocalDB)
3. Jalankan perintah berikut melalui Package Manager Console atau terminal:Bashdotnet ef database update
4. Jalankan Aplikasi
   Tekan F5 di Visual Studio atau jalankan:Bashdotnet run
   Akses dokumentasi interaktif melalui Swagger UI di http://localhost:5048/swagger

Daftar Endpoint API
Presensi (Transaksi Utama)
POST /api/Presensi/submit — Mencatat presensi mahasiswa dengan validasi lokasi & konkurensi

GET /api/Presensi — Menampilkan seluruh data rekap presensi beserta relasinya

GET /api/Presensi/{id} — Menampilkan detail presensi berdasarkan ID

PUT /api/Presensi/{id} — Memperbarui status (Hadir, Izin, Sakit, Alpha) & keterangan

DELETE /api/Presensi/{id} — Menghapus data presensi

Data Master (Mahasiswa & JadwalKuliah)
GET / POST /api/Mahasiswa — Melihat daftar atau menambahkan data mahasiswa

GET / PUT / DELETE /api/Mahasiswa/{nim} — Mengelola data mahasiswa berdasarkan NIM

GET / POST /api/JadwalKuliah — Melihat daftar atau menambahkan mata kuliah

GET / PUT / DELETE /api/JadwalKuliah/{id} — Mengelola data jadwal kuliah berdasarkan ID

Contoh Request & Pengujian Konkurensi
Contoh Payload POST /api/Presensi/submit
JSON
{
  "nimMahasiswa": "220001",
  "jadwalKuliahId": 1,
  "latitude": -7.4245,
  "longitude": 109.2291,
  "keterangan": "Hadir"
}
Pengujian Race Condition (PowerShell)
Pastikan aplikasi sedang berjalan, lalu eksekusi skrip pengujian paralel pada folder Tests:

PowerShell
.\Tests\test_race.ps1
Sistem akan menembakkan 10 request secara bersamaan dan hanya meloloskan 1 transaksi valid, 
sementara 9 lainnya ditolak dengan respons 409 Conflict.