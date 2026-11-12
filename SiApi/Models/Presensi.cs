using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiApi.Models
{
    public class Presensi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NimMahasiswa { get; set; } = string.Empty;

        [ForeignKey("NimMahasiswa")]
        public Mahasiswa? Mahasiswa { get; set; } // Foreign Key ke tabel Mahasiswa

        [Required]
        public int JadwalKuliahId { get; set; }

        [ForeignKey("JadwalKuliahId")]
        public JadwalKuliah? JadwalKuliah { get; set; } // Foreign Key ke tabel Jadwal

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public DateTime WaktuPresensi { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Hadir";
        public string? Keterangan { get; set; }
    }
}