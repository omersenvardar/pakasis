using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DBGoreWebApp.Models.Entities
{


   [Table("aracyilversiyon")]
    public class AracYilVersiyon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VersiyonID { get; set; }

        public int? MarkaID { get; set; }
        public int? ModelID { get; set; }
        public string? YilID { get; set; }

        [StringLength(255)]
        public string? VersiyonAdi { get; set; }
        public string? Ozellikleri { get; set; }
    }
}