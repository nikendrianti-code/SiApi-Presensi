using System.ComponentModel.DataAnnotations;

namespace SiApi.Models
{
    public class Presensi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NimMahasiswa { get; set; } = string.Empty;

        [Required]
        public string NamaMahasiswa { get; set; } = string.Empty; // Tambahan data

        [Required]
        public int JadwalKuliahId { get; set; }

        [Required]
        public string NamaMataKuliah { get; set; } = string.Empty; // Tambahan data

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public DateTime WaktuPresensi { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Hadir"; // Hadir, Izin, Sakit, Alpa

        public string? Keterangan { get; set; } // Tambahan data opsional
    }
}