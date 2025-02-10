using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Models.ViewModels
{
    public class ArsaSearchViewModel
    {
        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        public string? Mahalle { get; set; }

        [Required]
        public string Tur { get; set; } = string.Empty;

        [Required]
        public string Tip { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int? AreaMin { get; set; }

        [Range(0, int.MaxValue)]
        public int? AreaMax { get; set; }

        [Range(0, int.MaxValue)]
        public int? PriceMin { get; set; }

        [Range(0, int.MaxValue)]
        public int? PriceMax { get; set; }

        public List<Arsa>? Results { get; set; }
    }
}
