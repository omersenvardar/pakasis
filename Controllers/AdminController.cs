using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using DBGoreWebApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DBGoreWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using DBGoreWebApp.Services;
using DBGoreWebApp.Services.Arabalar;
using Microsoft.AspNetCore.Authorization;

namespace DBGoreWebApp.Controllers
{

    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ArabaBilgileriServices _arabaBilgileriServices;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger, ArabaBilgileriServices arabaBilgileriServices)
        {
            _context = context;
            _logger = logger;
            _arabaBilgileriServices = arabaBilgileriServices;


        }
        // Adres Gruplarını Hazırlama
        private Dictionary<string, Dictionary<string, List<string>>> GetAdresGruplari()
        {
            var adresler = _context.Adresler.ToList();
            var adresGruplari = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var adres in adresler)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                if (!adresGruplari.ContainsKey(adres.Il))
                {
                    adresGruplari[adres.Il] = new Dictionary<string, List<string>>();
                }
#pragma warning restore CS8604 // Possible null reference argument.

#pragma warning disable CS8604 // Possible null reference argument.
                if (!adresGruplari[adres.Il].ContainsKey(adres.Ilce))
                {
                    adresGruplari[adres.Il][adres.Ilce] = new List<string>();
                }
#pragma warning restore CS8604 // Possible null reference argument.

#pragma warning disable CS8604 // Possible null reference argument.
                adresGruplari[adres.Il][adres.Ilce].Add(adres.MahalleKoyAdi);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            return adresGruplari;
        }

        public void ArabaBilgileriServices()
        {
            var serializedData = "a:82:{s:10:\"pageNumber\";N;s:9:\"pageCount\";i:5;s:2:\"id\";i:58582;...}"; // Örnek veri
                                                                                                              // var arabaBilgileriServices =  ArabaBilgileriServices();

            // Veriyi parse et
            var parsedData = _arabaBilgileriServices.ParseSerializedData(serializedData);

            // Belirli bir özelliği alma
            var ortalamaYakit = _arabaBilgileriServices.GetOrtalamaYakit(serializedData);

            // Tüm özellikleri alma
            var allProperties = _arabaBilgileriServices.GetAllProperties(serializedData);
            

        }

        [HttpGet]
        public IActionResult Index()
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Emlak İlanları İstatistikleri
            ViewBag.AktifEmlak = _context.EmlakBahceler.Count(e => e.Durum == 'a');
            ViewBag.PasifEmlak = _context.EmlakBahceler.Count(e => e.Durum == 'p');
            ViewBag.OnayEmlak = _context.EmlakBahceler.Count(e => e.Durum == 'k');

            // Araç İlanları İstatistikleri
            ViewBag.AktifArac = _context.Arabalar.Count(); // Aktif durum 'a'

            return View();
        }


        public async Task<IActionResult> Kullanicilar()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Tüm kullanıcıları al
            var kullanicilar = await _context.Kullanicilar.ToListAsync();

            // Her kullanıcının ilan sayısını hesapla
            var emlakIlanSayilari = kullanicilar.ToDictionary(
                k => k.Id,
                k => _context.EmlakBahceler.Count(e => e.KullaniciId == k.Id)
            );

            var arabaIlanSayilari = kullanicilar.ToDictionary(
                k => k.Id,
                k => _context.Arabalar.Count(a => a.KullaniciId == k.Id)
            );

            // ViewBag üzerinden görünümde erişmek için verileri gönder
            ViewBag.EmlakIlanSayilari = emlakIlanSayilari;
            ViewBag.ArabaIlanSayilari = arabaIlanSayilari;

            return PartialView("Admin/_KullanicilarPartial", kullanicilar);
        }







        public async Task<IActionResult> EmlakIlanlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var emlakIlanlari = await _context.EmlakBahceler
                .Include(e => e.AdresKonumuNavigation) // Adres bilgilerini dahil et
                .ToListAsync();

            // Resimleri manuel olarak eşleştiriyoruz
            foreach (var ilan in emlakIlanlari)
            {
                ilan.ArsaResimleri = await _context.ArsaResimleri
                    .Where(r => r.ArsaId == ilan.IlanNo && r.IsDeleted == 0)
                    .ToListAsync();

            }
            return PartialView("Admin/_EmlakIlanlariPartial", emlakIlanlari);
        }




        public async Task<IActionResult> ArabaIlanlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var arabaIlanlari = await _context.Arabalar
            .Include(e => e.AdresKonumuNavigation)
            .ToListAsync();
            foreach (var ilan in arabaIlanlari)
            {
                ilan.ArabaResimleri = await _context.ArabaResimleri
                .Where(r => r.Id == ilan.Id && r.IsDeleted == 0)
                .ToListAsync();
            }
            return PartialView("Admin/_ArabaIlanlariPartial", arabaIlanlari);
        }

        // Kullanıcı Detayları Metodu

        public async Task<IActionResult> KullaniciDetay(int id)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Kullanıcıyı ilişkili ilanlarla beraber getiriyoruz
            var kullanici = await _context.Kullanicilar
                .Include(k => k.EmlakIlanlari)
                    .ThenInclude(e => e.AdresKonumuNavigation) // Emlak ilanlarının adres bilgileri
                .Include(k => k.ArabaIlanlari) // Kullanıcının araç ilanları
                .FirstOrDefaultAsync(k => k.Id == id);

            ViewBag.AbonelikTipleri = new SelectList(new[] { "Üye", "Bronz", "Gümüş", "Altın", "Platin", "Premium", "Vip" }, kullanici.AbonelikTipi);
            if (kullanici == null)
            {
                TempData["Message"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index");
            }

            // Kullanıcı detaylarını modele bağlayarak döndürüyoruz
            return View("KullaniciDetay", kullanici);
        }

        [HttpPost]
        public IActionResult KullaniciDetay(Kullanici kullanici)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var mevcutKullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == kullanici.Id);

            if (mevcutKullanici == null)
            {
                return NotFound();
            }
            mevcutKullanici.AbonelikTipi = kullanici.AbonelikTipi;
            _context.SaveChanges();

            return RedirectToAction("KullaniciDetay", new { id = kullanici.Id });
        }

        [HttpPost]
        public IActionResult UpdateIlanStatus(UpdateIlanStatusRequest request)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (request == null || request.Id <= 0 || string.IsNullOrEmpty(request.Status))
            {
                TempData["Message"] = "Geçersiz veri gönderildi.";
                return RedirectToAction("Index");
            }

            var ilan = _context.EmlakBahceler.FirstOrDefault(e => e.Id == request.Id);
            if (ilan == null)
            {
                TempData["Message"] = "İlan bulunamadı.";
                return RedirectToAction("Index");
            }

            // Durumu güncelleme
            ilan.Durum = request.Status.ToLower() switch
            {
                "aktif" => 'a',
                "pasif" => 'p',
                "kaldir" => 'k',
                _ => ilan.Durum // Geçersiz durum
            };

            _context.SaveChanges();

            TempData["Message"] = "İlan durumu başarıyla güncellendi.";
            return RedirectToAction("Index");
        }



        public class UpdateIlanStatusRequest
        {

            public int Id { get; set; }
            public string? Status { get; set; }
        }



        [HttpGet]
        public IActionResult GetEmlakIlanlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var emlakIlanlari = _context.EmlakBahceler
                .Include(e => e.AdresKonumuNavigation)
                .ToList();

            return PartialView("Admin_EmlakIlanlariPartial", emlakIlanlari);
        }

        [HttpGet]
        public IActionResult GetArabaIlanlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var arabaIlanlari = _context.Arabalar
                .Include(a => a.AdresKonumuNavigation)
                .ToList();

            return PartialView("Admin/_ArabaIlanlariPartial", arabaIlanlari);
        }


        // Fetch all listings (TumIlanlar)

        public async Task<IActionResult> TumIlanlar()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var tumIlanlar = await _context.EmlakBahceler
                .FromSqlRaw("SELECT * FROM emlak_bahce")
                .ToListAsync();

            return PartialView("Admin/_AdminTumIlanlarPartial", tumIlanlar);
        }

        [HttpGet]
        public async Task<IActionResult> AktifIlanlar()
        {
            var aktifIlanlar = await _context.EmlakBahceler
                .Include(e => e.AdresKonumuNavigation) // Adres bilgilerini dahil ediyoruz
                .Include(e => e.ArsaResimleri)         // Resim bilgilerini dahil ediyoruz
                .Where(e => e.Durum == 'a')  // Sadece aktif ilanları getiriyoruz
                .ToListAsync();

            return PartialView("Admin/_AdminAktifIlanlarPartial", aktifIlanlar);
        }


        // Fetch passive listings (PasifIlanlar)

        public async Task<IActionResult> PasifIlanlar()
        {
            // Pasif ilanları getiriyoruz.
            var pasifIlanlar = await _context.EmlakBahceler
                .Include(e => e.Kullanici)            // Kullanıcı bilgilerini dahil ediyoruz
                .Include(e => e.AdresKonumuNavigation) // Adres bilgilerini dahil ediyoruz
                .Include(e => e.ArsaResimleri)         // Resim bilgilerini dahil ediyoruz
                .Where(e => e.Durum == 'p')  // Sadece pasif ilanları alıyoruz
                .ToListAsync();

            return PartialView("Admin/_AdminPasifIlanlarPartial", pasifIlanlar);
        }

        public IActionResult EmlakDetay(int id)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // İlgili emlak ilanını detaylarıyla birlikte çekiyoruz
            var emlak = _context.EmlakBahceler
                .Include(e => e.Kullanici) // Kullanıcı bilgilerini dahil ediyoruz
                .Include(e => e.AdresKonumuNavigation) // Adres bilgilerini dahil ediyoruz
                .FirstOrDefault(e => e.Id == id);

            if (emlak != null)
            {
                // Resimleri ayrı bir sorguyla filtreliyoruz
#pragma warning disable CS8604 // Possible null reference argument.
                emlak.ArsaResimleri = _context.ArsaResimleri
                    .Where(r => r.ArsaId == emlak.IlanNo)
                    .Where(r => r.IsDeleted == 0)
                    .ToList();
#pragma warning restore CS8604 // Possible null reference argument.
            }

            ViewBag.AdresGruplari = GetAdresGruplari();
            if (emlak == null)
            {
                return NotFound("Emlak ilanı bulunamadı.");
            }

            return View(emlak);
        }
        //resim güncelleme için

        [HttpPost]
        public async Task<IActionResult> UpdateEmlakDetay(int id, EmlakBahce emlak, string Il, string Ilce, string MahalleKoyAdi, string[] silinecekResimler, List<IFormFile> yeniResimler)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var ilan = await _context.EmlakBahceler
                .Include(e => e.ArsaResimleri)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ilan == null)
            {
                TempData["UpdateEmlakDetayMessage"] = "Emlak ilanı bulunamadı.";
                return RedirectToAction("EmlakDetay", new { id = id });
            }

            try
            {
                // **İlan Bilgilerini Güncelle**
                ilan.IlanAdi = emlak.IlanAdi;
                ilan.FiyatSatis = emlak.FiyatSatis;
                ilan.Ada = emlak.Ada;
                ilan.Parsel = emlak.Parsel;
                ilan.YuzOlcum = emlak.YuzOlcum;
                ilan.Turu = !string.IsNullOrEmpty(emlak.Turu) ? emlak.Turu : "İlan";
                ilan.TarihBas = emlak.TarihBas;
                ilan.TarihBitis = emlak.TarihBitis;
                ilan.TarihTeslim = emlak.TarihTeslim;
                ilan.BagceSayisi = emlak.BagceSayisi;
                ilan.BahceSayisiIstenen = emlak.BahceSayisiIstenen;
                ilan.TaksitSayisi = emlak.TaksitSayisi;
                ilan.FiyatPesin = emlak.FiyatPesin;
                ilan.Kapora = emlak.Kapora;
                ilan.Taksitlimi = emlak.Taksitlimi;
                ilan.ParselSorgu = emlak.ParselSorgu;
                ilan.TaksitBedeli = emlak.TaksitBedeli;
                ilan.IlanAciklamasi = emlak.IlanAciklamasi;

                // **Adres Güncellemesi**
                if (!string.IsNullOrEmpty(Il) && !string.IsNullOrEmpty(Ilce) && !string.IsNullOrEmpty(MahalleKoyAdi))
                {
                    var yeniAdres = await _context.Adresler
                        .FirstOrDefaultAsync(a => a.Il == Il && a.Ilce == Ilce && a.MahalleKoyAdi == MahalleKoyAdi);

                    if (yeniAdres != null)
                    {
                        ilan.AdresKonumu = yeniAdres.Id;
                    }
                }
            }
            catch (Exception ex)
            {
            }


            // **Resimleri Güncelle**
            var fileService = new FileService(_context);
            // Silinecek resimleri işaretle
            await fileService.SilKayitlariIsDeletedAsync<ArsaResim>(silinecekResimler);


            // **📌 Yeni Resimleri Yükle**
            if (ilan.IlanNo.HasValue)
            {
                await fileService.YukleYeniResimlerAsync<ArsaResim>(yeniResimler, ilan.IlanNo.Value, "wwwroot/emlakresimler");
            }
            else
            {
                // İlana ait IlanNo null ise yapılacak işlemler
                // Örneğin: bir hata mesajı veya loglama işlemi
                throw new InvalidOperationException("IlanNo null değer içeriyor.");
            }


            ilan.Durum = 'p';

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }

            TempData["UpdateEmlakDetayMessage"] = "Emlak ilanı başarıyla güncellendi.";
            return RedirectToAction("EmlakDetay", new { id = id });
        }

        [HttpPost]
        public IActionResult UpdateEmlakDurum(int id, char durum)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var emlak = _context.EmlakBahceler.FirstOrDefault(e => e.Id == id);
            if (emlak == null)
            {
                TempData["Message"] = "Emlak ilanı bulunamadı.";
                return RedirectToAction("Index");
            }

            emlak.Durum = durum;
            _context.SaveChanges();

            TempData["Message"] = "Emlak ilan durumu başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateKullaniciDurum(int id, char durum)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == id);
            if (kullanici == null)
            {
                TempData["Message"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index");

            }

            kullanici.Durum = durum;
            _context.SaveChanges();

            TempData["Message"] = "Kullanıcı durumu başarıyla güncellendi.";
            return RedirectToAction("Index");
        }




        [HttpGet]
        public JsonResult GetModels(int markaId)
        {

            var models = _context.AracModelListesis
                .Where(m => m.MarkaID == markaId)
                .Select(m => new { m.ModelID, m.Model })
                .ToList();
            return Json(models);
        }
        [HttpGet]
        public JsonResult GetVersions(int yilId)
        {
            var versions = _context.AracYilVersiyons
                .Where(v => v.YilID == yilId.ToString())
                .Select(v => new { v.VersiyonID, v.VersiyonAdi, v.Ozellikleri })
                .ToList();
            return Json(versions);
        }
        [HttpGet]
        public JsonResult GetYears1(int modelId)
        {
            var years = _context.AracModelYillaris
                .Where(y => y.ModelID == modelId)
                .Select(y => new
                {
                    y.YilID,
                    Yil = y.Yil.HasValue ? y.Yil.Value : 0 // Eğer Yil boşsa 0 döndür
                })
                .ToList();
            return Json(years);
        }

        [HttpGet]
        public JsonResult GetYears(int modelId)
        {
            var years = _context.AracModelYillaris
                .Where(y => y.ModelID == modelId)
                .Select(y => new { y.YilID, y.Yil }) // YilID ve Yil alanlarını seçiyoruz
                .ToList();

            return Json(years);
        }

        public IActionResult ArabaDetay(int id)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var araba = _context.Arabalar
            .Include(a => a.Kullanici)
            .Include(a => a.AdresKonumuNavigation)
            .Select(a => new Araba
            {
                Id = a.Id,
                Kullanici = a.Kullanici,
                AdresKonumuNavigation = a.AdresKonumuNavigation,
                MarkaID = a.MarkaID,
                ModelID = a.ModelID,
                YilID = a.YilID,
                VersiyonID = a.VersiyonID,
                VersiyonAdi = a.VersiyonAdi,
                Fiyat = a.Fiyat,
                Ozellikleri = a.Ozellikleri,
                ArabaResimleri = a.ArabaResimleri.Where(r => r.IsDeleted == 0 && r.ArabaId == a.Id).ToList()
            })
            .FirstOrDefault(a => a.Id == id);


            if (araba == null)
            {
                return NotFound("Araç ilanı bulunamadı.");
            }

            // AracModelYillari sorgusu
            var aracYil = _context.AracModelYillaris.FirstOrDefault(a => a.ModelID == araba.ModelID);

            // ViewModel hazırlığı
            var model = new ArabaViewModel
            {
                ArabaDetay = araba,
                KullaniciDetay = _context.Kullanicilar.FirstOrDefault(k => k.Id == araba.KullaniciId),
                Adresler = _context.Adresler.ToList(),
                AracMarkalaris = _context.AracMarkalaris.ToList(),
                AracModelListesis = _context.AracModelListesis.Where(m => m.MarkaID == araba.MarkaID).ToList(),
                AracModelYillaris = _context.AracModelYillaris.Where(y => y.ModelID == araba.ModelID).ToList(),
                AracYilVersiyons = _context.AracYilVersiyons.Where(v => v.YilID == araba.YilID.ToString()).ToList(),
            };

            // Adres Grupları
            ViewBag.AdresGruplari = GetAdresGruplari();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateArabaDetay(int id, ArabaViewModel model, List<IFormFile> yeniResimler, string[] silinecekResimler, string Il, string Ilce, string MahalleKoyAdi)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var araba = await _context.Arabalar
                .Include(a => a.ArabaResimleri)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (araba == null)
            {
                TempData["UpdateArabaDetayMessage"] = "Araç ilanı bulunamadı.";
                return RedirectToAction("Index");
            }

            // **Adres Güncelleme**
            var adres = _context.Adresler.FirstOrDefault(a => a.Il == Il && a.Ilce == Ilce && a.MahalleKoyAdi == MahalleKoyAdi);
            if (adres != null)
            {
                araba.AdresKonumu = adres.Id;
            }
            // **Araba Bilgilerini Güncelle**
            araba.MarkaID = model.MarkaID;
            araba.ModelID = model.ModelID;
            araba.YilID = model.YilID;
            araba.VersiyonID = model.VersiyonID;
            araba.VersiyonAdi = model.VersiyonAdi;
            araba.Ozellikleri = model.Ozellikleri;
            araba.Fiyat = model.Fiyat;
            araba.Durum = 'p'; // Durumu güncelleyebilirsiniz

            // **Resimleri Güncelle**
            var fileService = new FileService(_context);
            await fileService.SilKayitlariIsDeletedAsync<ArabaResim>(silinecekResimler);

            if (yeniResimler != null && yeniResimler.Count > 0)
            {
                await fileService.YukleYeniResimlerAsync<ArabaResim>(yeniResimler, araba.Id, "wwwroot/resimaraba");
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["UpdateArabaDetayMessage"] = "Araç ilanı başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["UpdateArabaDetayMessage"] = "Araç ilanı güncellenirken bir hata oluştu.";
            }

            return RedirectToAction("ArabaDetay", new { id = araba.Id });
        }



        [HttpPost]
        public IActionResult UpdateArabaDurum(int id, char durum)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var araba = _context.Arabalar.FirstOrDefault(a => a.Id == id);

            if (araba == null)
            {
                TempData["UpdateArabaDetayMessage"] = "Araç ilanı bulunamadı.";
                return RedirectToAction("Index");
            }

            araba.Durum = durum;
            _context.SaveChanges();

            TempData["UpdateArabaDetayMessage"] = "Araç ilan durumu başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateKullaniciAbonelik(int Id, string AbonelikTipi, DateTime? AbonelikBaslangicTarihi, DateTime? AbonelikBitisTarihi, DateTime? SonKullanimTarihi)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            
            // Kullanıcıyı veritabanında bul
            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == Id);

            if (kullanici == null)
            {
                TempData["Message"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Kullanicilar");
            }

            // Abonelik bilgilerini güncelle
            try
            {
                kullanici.AbonelikTipi = AbonelikTipi;
                kullanici.AbonelikBaslangicTarihi = AbonelikBaslangicTarihi;
                kullanici.AbonelikBitisTarihi = AbonelikBitisTarihi;
                kullanici.SonKullanimTarihi = SonKullanimTarihi;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Güncelleme sırasında bir hata oluştu.";
                return RedirectToAction("Kullanicilar");
            }

            // Veritabanında değişiklikleri kaydet
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Kaydetme işlemi sırasında bir hata oluştu.";
                return RedirectToAction("Kullanicilar");
            }

            // Başarı mesajını TempData ile gönder
            TempData["Message"] = "Kullanıcı abonelik bilgileri başarıyla güncellendi.";
            return RedirectToAction("Guncellendi");
        }
        public async Task<IActionResult> Guncellendi()
        {
            return await Task.Run(() => PartialView("Admin/_AdminKullaniciGuncellendiPartial"));
        }


        public async Task<IActionResult> KaldirilanIlanlar()
        {
            var kaldirilanIlanlar = await _context.EmlakBahceler
                .FromSqlRaw("SELECT * FROM emlak_bahce WHERE ilanstatusu = 'kaldırıldı'")
                .ToListAsync();

            return PartialView("Admin/_AdminKaldirilanIlanlarPartial", kaldirilanIlanlar);
        }

        // Fetch listings waiting for approval (OnayBekleyenIlanlar)
        public async Task<IActionResult> OnayBekleyenIlanlar()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var onayBekleyenIlanlar = await _context.EmlakBahceler
                .Where(x => x.Durum == 'p')
                .ToListAsync();

            return PartialView("Admin/_AdminOnayBekleyenIlanlarPartial", onayBekleyenIlanlar);
        }


        // Ayarları düzenlemek için

        [HttpPost]
        public IActionResult UpdateAyar(int id, string settingKey, string settingValue)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var ayar = _context.AdminSettings.FirstOrDefault(s => s.SettingId == id);
            if (ayar == null)
            {
                TempData["Message"] = "Ayar bulunamadı.";
                return RedirectToAction("SayfaAyarlari");
            }

            ayar.SettingKey = settingKey;
            ayar.SettingValue = settingValue;

            _context.SaveChanges();

            TempData["Message"] = "Ayar başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        // Yeni bir ayar eklemek için

        [HttpPost]
        public IActionResult AddAyar(string settingKey, string settingValue)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var yeniAyar = new AdminSettings
            {
                SettingKey = settingKey,
                SettingValue = settingValue
            };

            _context.AdminSettings.Add(yeniAyar);
            _context.SaveChanges();

            TempData["Message"] = "Yeni ayar başarıyla eklendi.";

            // `SayfaAyarlari` metoduna yönlendirme yapılıyor
            return RedirectToAction("Index");
        }


        // Bir ayarı silmek için

        [HttpPost]
        public IActionResult DeleteAyar(int id)
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var ayar = _context.AdminSettings.FirstOrDefault(s => s.SettingId == id);
            if (ayar == null)
            {
                TempData["Message"] = "Ayar bulunamadı.";
                return RedirectToAction("SayfaAyarlari");
            }

            _context.AdminSettings.Remove(ayar);
            _context.SaveChanges();

            TempData["Message"] = "Ayar başarıyla silindi.";
            return RedirectToAction("SayfaAyarlari");
        }
        // admin sayfa ayarları sonu
        [HttpGet("/Admin/Ayarlar/SayfaAyarlari")]
        public IActionResult SayfaAyarlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var ayarlar = _context.AdminSettings.ToList();
            return PartialView("Admin/Ayarlar/_SayfaAyarlari", ayarlar);
        }

        [HttpGet("/Admin/Ayarlar/VitrinIlanlari")]
        public IActionResult VitrinIlanlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return PartialView("Admin/Ayarlar/_VitrinIlanlari");
        }
        [HttpGet("/Admin/Ayarlar/KategoriAyarlari")]
        public IActionResult KategoriAyarlari()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            return PartialView("Admin/Ayarlar/_Kategoriler", _context.Kategoriler.ToList());
        }

        // Handling error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}




