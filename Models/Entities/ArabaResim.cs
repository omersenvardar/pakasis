using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    public class ArabaResim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Otomatik Artan ID
        public int Id { get; set; }

        [Required]
        public int ArabaId { get; set; } // Araba tablosuyla ilişki

        [StringLength(255)]
        public string? ResimArabaUrl { get; set; } // Resmin URL'si

        [StringLength(100)]
        public string? ResimAdi { get; set; } // Resmin adı

        public byte IsDeleted { get; set; } // Varsayılan olarak silinmemiş

        // Navigasyon Özelliği
        [ForeignKey("ArabaId")]
        public virtual Araba? Araba { get; set; } // Araba ile ilişki
        
    }
}
