using System.ComponentModel.DataAnnotations;

namespace SiApi.Models
{
    public class Mahasiswa
    {
        [Key]
        public string Nim { get; set; }

        [Required]
        public string NamaLengkap { get; set; }

        [Required]
        public string Kelas { get; set; }
    }
}