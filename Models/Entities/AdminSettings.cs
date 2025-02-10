using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    public class AdminSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Otomatik artan birincil anahtar
        public int SettingId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "SettingKey en fazla 100 karakter olabilir.")]
        public string? SettingKey { get; set; } // Ayar anahtarı

        [Required]
        [StringLength(255, ErrorMessage = "SettingValue en fazla 255 karakter olabilir.")]
        public string? SettingValue { get; set; } // Ayar değeri
    }
}
