using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SiApi.Models
{
    public class JadwalKuliah
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public string NamaMataKuliah { get; set; } = string.Empty;

        // Relasi (1 Jadwal bisa punya banyak Presensi)
        [JsonIgnore]
        public ICollection<Presensi>? DaftarPresensi { get; set; }
    }
}
