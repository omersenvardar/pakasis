using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    [Table("kategori")]
    public class Kategori
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string KategoriAd { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AltKategori { get; set; } = string.Empty;

        [StringLength(128)]
        public string Controller { get; set; } = string.Empty;

        [StringLength(128)]
        public string Action { get; set; } = string.Empty;
    }
}


// _context.Kategoriler.Add(new Kategori
// {
//     KategoriAd = "Emlak",
//     AltKategori = "Villa",
//     Controller = "EmlakBahce",
//     Action = "Villa"
// });
// _context.SaveChanges();
