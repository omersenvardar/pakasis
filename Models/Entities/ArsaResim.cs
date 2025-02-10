using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    [Table("arsaresimleri")]
    public class ArsaResim
    {
        [Key]
        public int Id { get; set; }

        public int ArsaId { get; set; }

        [StringLength(255)]
        public string? ResimArsaUrl { get; set; }

        [StringLength(100)]
        public string? ResimAdi { get; set; }

        // 'tinyint(1)' için byte tipi kullanıldı
        public byte IsDeleted { get; set; }

        /// EmlakBahce ile ilişki
    public virtual EmlakBahce? EmlakBahce { get; set; }
    }
}
