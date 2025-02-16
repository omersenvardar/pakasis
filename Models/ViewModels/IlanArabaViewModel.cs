using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBGoreWebApp.Models.ViewModels
{
    public class IlanArabaViewModel
{
    public int Id { get; set; }
    public string? VersiyonAdi { get; set; }
    public string? Marka { get; set; }
    public string? Model { get; set; }
    public int Yil { get; set; }
    public int? Fiyat { get; set; }
    public char Durum { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ResimUrl { get; set; }
}

}