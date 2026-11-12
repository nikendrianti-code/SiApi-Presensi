using System.ComponentModel.DataAnnotations;

namespace SiApi.Models
{
    public class Mahasiswa
    {
        [Key]
        public string Nim { get; set; } = string.Empty; // Primary Key

        [Required]
        public string Nama { get; set; } = string.Empty;

        // Relasi (1 Mahasiswa bisa punya banyak Presensi)
        public ICollection<Presensi>? DaftarPresensi { get; set; }
    }
}