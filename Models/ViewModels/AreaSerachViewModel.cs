using System.ComponentModel.DataAnnotations;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Models.ViewModels
{
public class AreaSearchViewModel
{
    [Required]
    public string? City { get; set; }
    [Required]
    public string? District { get; set; }
    public string? Mahalle { get; set; }
    [Required]
    public string? Tur { get; set; }
    [Required]
    public string? Tip { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Lütfen {1} veya daha büyük bir değer giriniz.")]
    public int? AreaMin { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Lütfen {1} veya daha büyük bir değer giriniz.")]
    public int? AreaMax { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Lütfen {1} veya daha büyük bir değer giriniz.")]
    public int? PriceMin { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Lütfen {1} veya daha büyük bir değer giriniz.")]
    public int? PriceMax { get; set; }
    public List<Arsa>? Results { get; set; } // Sonuç listesi
}
}