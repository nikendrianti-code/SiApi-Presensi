using System.ComponentModel.DataAnnotations;

namespace SiApi.Models
{
    public class JadwalKuliah
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public string NamaMataKuliah { get; set; } = string.Empty;

        // Relasi (1 Jadwal bisa punya banyak Presensi)
        public ICollection<Presensi>? DaftarPresensi { get; set; }
    }
}
