using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace DBGoreWebApp.Models.Entities
{

    [Table("aracmodelyillari")]
    public class AracModelYillari
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int YilID { get; set; }

        public int? MarkaID { get; set; }

        public int? ModelID { get; set; }

        public int? Yil { get; set; }
    }
}