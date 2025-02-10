using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    [Table("il_ilce_mah")]
    public class Adres
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("il")]
        [MaxLength(25)]
        public string? Il { get; set; }
        
        [Column("ilçe")]
        [MaxLength(50)]
        public string? Ilce { get; set; }
        
        [Column("MahalleKoyAdi")]
        [MaxLength(75)]
        public string? MahalleKoyAdi { get; set; }
    }
}
