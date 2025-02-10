using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBGoreWebApp.Models.ViewModels
{
    public class KullaniciViewModel
    {
        public int Id { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? Telefon { get; set; }
        public string? Email { get; set; }
        public string? ImgUrl { get; set; }
        public char Durum { get; set; }
        public string? FirmaUnvani { get; set; }
        public string? AbonelikTipi { get; set; }
        public DateTime? AbonelikBaslangicTarihi { get; set; }
        public DateTime? AbonelikBitisTarihi { get; set; }
        public string? AbonelikStatusu { get; set; }
        public DateTime? SonKullanimTarihi { get; set; }
    }
}