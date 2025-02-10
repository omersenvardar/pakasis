using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    public class Araba
    {
        [Key]
        public int Id { get; set; } // Otomatik Artan Primary Key

        // Kullanıcıyla ilişki
        [Required]
        public int KullaniciId { get; set; }
        [ForeignKey("KullaniciId")]
        public virtual Kullanici? Kullanici { get; set; }

        // Marka ile ilişki
        [Required]
        public int MarkaID { get; set; }
        [ForeignKey("MarkaID")]
        public virtual AracMarkalari? Marka { get; set; }

        // Model ile ilişki
        [Required]
        public int ModelID { get; set; }
        [ForeignKey("ModelID")]
        public virtual AracModelListesi? Model { get; set; }

        // Yıl ile ilişki
        [Required]
        public int YilID { get; set; }
        [ForeignKey("YilID")]
        public virtual AracModelYillari? Yil { get; set; }

        // Versiyon ile ilişki
        [Required]
        public int VersiyonID { get; set; }
        [ForeignKey("VersiyonID")]
        public virtual AracYilVersiyon? Versiyon { get; set; }

        // Adres ile ilişki
        [Required]
        public int AdresKonumu { get; set; }
        [ForeignKey("AdresKonumu")]
        public virtual Adres? AdresKonumuNavigation { get; set; }

        // Araç Özellikleri
        [MaxLength(255)]
        public string? VersiyonAdi { get; set; } // Versiyon Adı
        [Column(TypeName = "text")]
        public string? Ozellikleri { get; set; } // Araç Özellikleri (detaylı açıklamalar)

        // Fiyat Bilgisi
        public int? Fiyat { get; set; } // Araç Fiyatı

        // Durum
        [Required]
        public char Durum { get; set; } = 'p'; // Varsayılan olarak 'p' (pending)
        
        // public byte IsDeleted { get; set; } // 'tinyint(1)' için byte tipi kullanıldı

        // Resimlerle ilişki
        public virtual ICollection<ArabaResim> ArabaResimleri { get; set; } = new List<ArabaResim>();

        // Diğer Alanlar (Opsiyonel)
        public DateTime? KayitTarihi { get; set; } = DateTime.Now; // Kayıt tarihi varsayılan olarak şu an
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
