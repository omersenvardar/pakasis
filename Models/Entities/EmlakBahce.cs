using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBGoreWebApp.Models.Entities
{
    [Table("emlak_bahce")]
    public class EmlakBahce
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("ilanno")]
        public int? IlanNo { get; set; }

        [Column("kullaniciid")]
        public int? KullaniciId { get; set; }

        [Column("ilanadi")]
        [MaxLength(100)]
        public string? IlanAdi { get; set; }

        [Column("adreskonumu")]
        public int? AdresKonumu { get; set; }

        [Column("ada")]
        public int? Ada { get; set; }

        [Column("parsel")]
        public short? Parsel { get; set; }

        [Column("yuzolcum")]
        public int? YuzOlcum { get; set; }

        [Column("hisse")]
        public int? Hisse { get; set; }

        [Column("türü")]
        [MaxLength(20)]
        public string? Turu { get; set; }

        [Column("tarih_bas")]
        public DateTime? TarihBas { get; set; }

        [Column("tarih_bitis")]
        public DateTime? TarihBitis { get; set; }

        [Column("tarih_teslim")]
        public DateTime? TarihTeslim { get; set; }

        [Column("fiyat_satis")]
        public int? FiyatSatis { get; set; }

        [Column("fiyat_pesin")]
        public int? FiyatPesin { get; set; }

        [Column("kapora")]
        public int? Kapora { get; set; }

        [Column("taksitlimi")]
        [MaxLength(1)]
        public string? Taksitlimi { get; set; }

        [Column("taksit_bedeli")]
        public string? TaksitBedeli { get; set; }

        [Column("taksit_sayisi")]
        public string? TaksitSayisi { get; set; }

        [Column("bagcesayisi")]
        public short? BagceSayisi { get; set; }

        [Column("bahcesayisisatilan")]
        public short? BahceSayisiIstenen { get; set; }

        [Column("ilanaciklamasi")]
        public string? IlanAciklamasi { get; set; }

        [Column("parselsorgu")]
        [MaxLength(255)]
        public string? ParselSorgu { get; set; }

        [Column("ilanturu")]
        [MaxLength(1)]
        public string? IlanTuru { get; set; }

        [Column("Durum")]
        public char Durum { get; set; } = 'p'; // Durum (Varsayılan: 'p')

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [ForeignKey("AdresKonumu")]
        public virtual Adres? AdresKonumuNavigation { get; set; }

        public virtual ICollection<ArsaResim>? ArsaResimleri { get; set; }
                                                  // Kullanıcı ile ilişki
        [ForeignKey("KullaniciId")]
        public virtual Kullanici? Kullanici { get; set; }
    }
}
