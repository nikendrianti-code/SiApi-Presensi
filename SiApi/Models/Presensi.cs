using System.ComponentModel.DataAnnotations;

namespace SiApi.Models
{
    public class Presensi
    {
        [Key]
        public int Id { get; set; } // Primary Key otomatis

        public string NimMahasiswa { get; set; } = string.Empty;
        public int JadwalKuliahId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime WaktuPresensi { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}