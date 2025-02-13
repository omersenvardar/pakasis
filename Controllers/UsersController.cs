using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DBGoreWebApp.Data;
using DBGoreWebApp.Models.Entities;
using DBGoreWebApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using DBGoreWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using DBGoreWebApp.Filters;

namespace DBGoreWebApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
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
        // Kullanıcıların Listesi
        [RoleAuthorize("admin", "üye")]
        public IActionResult Index()
        {
            var users = _context.Kullanicilar.ToList();
            return View(users);
        }

        // Kullanıcı Detayı
        [RoleAuthorize("admin", "üye")]
        public IActionResult Detail(int id)
        {
            var kullaniciAd = HttpContext.Session.GetString("KullaniciAd");
            var kullaniciId = HttpContext.Session.GetString("KullaniciId");

            

            var emlakIlanlari = _context.EmlakBahceler
               .Where(e => e.KullaniciId == id)
               .OrderByDescending(e => e.CreatedDate)
               .ToList(); // Tüm ilanları liste olarak getir
            var arabaIlanlari = _context.Arabalar
               .Where(a => a.KullaniciId == id)
               .OrderByDescending(a => a.CreatedDate)
               .ToList(); // Tüm ilanları liste olarak getir
               
            // Kullanıcının toplam ilanları (emlak ve araba ayrı ayrı)
            ViewBag.AktifEmlakIlanlarim = emlakIlanlari.Count(e => e.Durum == 'a' || e.Durum == 'v');
            ViewBag.PasifEmlakIlanlarim = emlakIlanlari.Count(e => e.KullaniciId == id && e.Durum == 'p');



            ViewBag.AktifArabaIlanlarim = arabaIlanlari.Count(a => a.KullaniciId == id && a.Durum == 'a' || a.Durum == 'v');
            ViewBag.PasifArabaIlanlarim = arabaIlanlari.Count(a => a.KullaniciId == id && a.Durum == 'p');

            var user = _context.Kullanicilar.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public async Task<IActionResult> Profil()
        {
            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");

            if (int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                var model = await _context.Kullanicilar.FindAsync(kullaniciId);
                if (model != null)
                {
                    return PartialView("~/Views/Shared/Kullanici/_Profil.cshtml", model);
                }
                else
                {
                    return NotFound("Kullanıcı bulunamadı.");
                }
            }
            else
            {
                return BadRequest("Kullanıcı Id geçersiz.");
            }
        }


        public async Task<IActionResult> Ilanlarim(string durum, string type)
        {
            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin" && HttpContext.Session.GetString("KullaniciYetki") != "üye")
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var adres = GetAdresGruplari();
            var ArsaResimleri = await _context.ArsaResimleri.ToListAsync();

            if (int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(durum))
                {
                    return BadRequest("Geçersiz filtre parametreleri.");
                }
                if (type == "emlak")
                {

                    // İlanları doğru filtreleme ile al
                    List<EmlakBahce> ilanlar = _context.EmlakBahceler
                        .Where(i => i.KullaniciId == kullaniciId && (i.Durum == durum[0])) // Öncelik düzeltildi
                        .ToList();
                    // İlk ArsaResimleri'ni al ve ViewBag'e ekle
                    var ilkResimler = await _context.ArsaResimleri
                        .Where(ar => ar.IsDeleted == 0 && ilanlar.Select(i => i.IlanNo).Contains(ar.ArsaId))
                        .GroupBy(ar => ar.ArsaId)
                        .Select(g => g.FirstOrDefault())
                        .ToListAsync();

                    // İlanları ve ilk resimleri ViewBag'e ekle
                    ViewBag.IlkResimler = ilkResimler;

                    return PartialView("~/Views/Shared/Kullanici/_Ilanlarim.cshtml", ilanlar);
                }
                else if (type == "araba")
                {
                    List<Araba> ilanlar = _context.Arabalar
                        .Where(i => i.KullaniciId == kullaniciId && i.Durum == durum[0]) // 'a' veya 'p'
                        .ToList();
                    var arabaResim = await _context.ArabaResimleri
                        .Where(ar => ar.IsDeleted == 0 && ilanlar.Select(i => i.Id).Contains(ar.ArabaId))
                        .GroupBy(ar => ar.ArabaId)
                        .Select(g => g.FirstOrDefault())
                        .ToListAsync();

                    // İlanları ve ilk resimleri ViewBag'e ekle
                    ViewBag.arabaResim = arabaResim;
                    return PartialView("~/Views/Shared/Kullanici/_ArabaIlanlarim.cshtml", ilanlar);
                }
            }

            return BadRequest("Kullanıcı oturumu geçersiz.");
        }
        public async Task<IActionResult> EmlakIlanlarimOnayli()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin" && HttpContext.Session.GetString("KullaniciYetki") != "üye")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");

            if (!int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                return BadRequest("Geçersiz Kullanıcı ID!");
            }
            var adres = GetAdresGruplari();
            var emlakIlanlari = _context.EmlakBahceler
               .Where(e => e.KullaniciId == kullaniciId && (e.Durum == 'a' || e.Durum == 'v'))
               .OrderByDescending(e => e.CreatedDate)
               .ToList(); // Tüm ilanları liste olarak getir
            
            var ilkResimler = await _context.ArsaResimleri
                .Where(ar => ar.IsDeleted == 0 && emlakIlanlari.Select(i => i.IlanNo).Contains(ar.ArsaId))
                .GroupBy(ar => ar.ArsaId)
                .Select(g => g.FirstOrDefault())
                .ToListAsync();
            // İlanları ve ilk resimleri ViewBag'e ekle
            ViewBag.IlkResimler = ilkResimler;
            return PartialView("~/Views/Shared/Kullanici/IlanEmlak/_EmlakOnayli.cshtml", emlakIlanlari);
        }


        public async Task<IActionResult> EmlakIlanlarimOnaysiz()
        {
             if (HttpContext.Session.GetString("KullaniciYetki") != "admin" && HttpContext.Session.GetString("KullaniciYetki") != "üye")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");

            if (!int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                return BadRequest("Geçersiz Kullanıcı ID!");
            }
            var adres = GetAdresGruplari();
            var emlakIlanlari = _context.EmlakBahceler
               .Where(e => e.KullaniciId == kullaniciId && (e.Durum == 'p'))
               .OrderByDescending(e => e.CreatedDate)
               .ToList(); // Tüm ilanları liste olarak getir
            
            var ilkResimler = await _context.ArsaResimleri
                .Where(ar => ar.IsDeleted == 0 && emlakIlanlari.Select(i => i.IlanNo).Contains(ar.ArsaId))
                .GroupBy(ar => ar.ArsaId)
                .Select(g => g.FirstOrDefault())
                .ToListAsync();
            // İlanları ve ilk resimleri ViewBag'e ekle
            ViewBag.IlkResimler = ilkResimler;
            return PartialView("~/Views/Shared/Kullanici/IlanEmlak/_EmlakOnaysiz.cshtml",emlakIlanlari);
        }

        public async Task<IActionResult> ArabaIlanlarimOnayli()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin" && HttpContext.Session.GetString("KullaniciYetki") != "üye")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");

            if (!int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                return BadRequest("Geçersiz Kullanıcı ID!");
            }
            var adres = GetAdresGruplari();
            
            
            List<Araba> ilanlar = _context.Arabalar
                        .Where(i => i.KullaniciId == kullaniciId && (i.Durum == 'a' || i.Durum == 'v')) // 'a' veya 'p'
                        .ToList();
                        
            var arabaResim = await _context.ArabaResimleri
                        .Where(ar => ar.IsDeleted == 0 && ilanlar.Select(i => i.Id).Contains(ar.ArabaId))
                        .GroupBy(ar => ar.ArabaId)
                        .Select(g => g.FirstOrDefault())
                        .ToListAsync();
            // İlanları ve ilk resimleri ViewBag'e ekle
            ViewBag.arabaResimleri = arabaResim;
            return PartialView("~/Views/Shared/Kullanici/IlanAraba/_ArabaOnayli.cshtml", ilanlar);
        }

        public async Task <IActionResult> ArabaIlanlarimOnaysiz()
        {
            if (HttpContext.Session.GetString("KullaniciYetki") != "admin" && HttpContext.Session.GetString("KullaniciYetki") != "üye")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");

            if (!int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                return BadRequest("Geçersiz Kullanıcı ID!");
            }
            var adres = GetAdresGruplari();
            
            
            List<Araba> ilanlar = _context.Arabalar
                        .Where(i => i.KullaniciId == kullaniciId && i.Durum == 'p') // 'a' veya 'p'
                        .ToList();
                        
            var arabaResim = await _context.ArabaResimleri
                        .Where(ar => ar.IsDeleted == 0 && ilanlar.Select(i => i.Id).Contains(ar.ArabaId))
                        .GroupBy(ar => ar.ArabaId)
                        .Select(g => g.FirstOrDefault())
                        .ToListAsync();
            // İlanları ve ilk resimleri ViewBag'e ekle
            ViewBag.arabaResimleri = arabaResim;
            return PartialView("~/Views/Shared/Kullanici/IlanAraba/_ArabaOnaysiz.cshtml",ilanlar);
        }

        public async Task<IActionResult> GetFirstArsaResimi(int id)
        {
            // Resimleri manuel olarak eşleştiriyoruz
            var ArsaResim = await _context.ArsaResimleri.FirstAsync(r => r.ArsaId == id);
            return View(ArsaResim);
        }

        public async Task<IActionResult> EditEmlak(int id)
        {
            var emlak = _context.EmlakBahceler
                //.Include(e => e.ArsaResimleri)
                .Include(e => e.AdresKonumuNavigation)
                .FirstOrDefault(e => e.Id == id);

            if (emlak == null)
            {
                TempData["Message"] = "Emlak ilanı bulunamadı.";
                return RedirectToAction("Index");
            }
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
            // Adres bilgilerini ViewBag'e ekle
            ViewBag.AdresGruplari = GetAdresGruplari();
            //return PartialView("~/Views/Shared/Kullanici/Emlak/_EditEmlakPartial.cshtml", emlak);
            return View("EditEmlak", emlak);
        }

        [HttpPost]
        public async Task<IActionResult> EditEmlak(EmlakBahce emlak, string Il, string Ilce, string MahalleKoyAdi, string[] silinecekResimler, List<IFormFile> yeniResimler)
        {
            // **1. İlanı Veritabanından Bulma**
            // İlgili ilanı veritabanından getiriyoruz. Eğer ilan bulunamazsa, hata mesajı gösterilip kullanıcı detayına yönlendirilir.
            var ilan = await _context.EmlakBahceler
                .Include(e => e.ArsaResimleri) // Resim bilgilerini de dahil ediyoruz.
                .FirstOrDefaultAsync(e => e.Id == emlak.Id);

            if (ilan == null)
            {
                // Eğer ilan bulunamazsa, kullanıcıya bir hata mesajı ver ve kullanıcı detay sayfasına dön.
                TempData["UpdateEmlakDetayMessage"] = "Emlak ilanı bulunamadı.";
                return RedirectToAction("Detail", new { id = emlak.KullaniciId });
            }

            // **2. Adres Güncelleme**
            // İl, ilçe ve mahalle bilgisine göre adres tablosundan uygun kaydı arıyoruz.
            var adres = _context.Adresler.FirstOrDefault(a => a.Il == Il && a.Ilce == Ilce && a.MahalleKoyAdi == MahalleKoyAdi);
            if (adres != null)
            {
                // Adres bulunduysa, ilan tablosundaki `AdresKonumu` alanını bu adresin ID'si ile güncelliyoruz.
                ilan.AdresKonumu = adres.Id;
            }

            // **3. Emlak Bilgilerini Güncelleme**
            // Kullanıcı tarafından gönderilen formdaki tüm değerleri mevcut ilan kaydına işliyoruz.
            ilan.IlanAdi = emlak.IlanAdi; // İlan başlığını güncelle.
            ilan.FiyatSatis = emlak.FiyatSatis; // Satış fiyatını güncelle.
            ilan.FiyatPesin = emlak.FiyatPesin; // Peşin fiyatını güncelle.
            ilan.Kapora = emlak.Kapora; // Kapora miktarını güncelle.
            ilan.Ada = emlak.Ada; // Ada numarasını güncelle.
            ilan.Parsel = emlak.Parsel; // Parsel numarasını güncelle.
            ilan.YuzOlcum = emlak.YuzOlcum; // Yüz ölçümü güncelle.
            ilan.Turu = !string.IsNullOrEmpty(emlak.Turu) ? emlak.Turu : "İlan"; // Emlak türünü (arsa, tarla vb.) güncelle.
            ilan.TarihBas = emlak.TarihBas; // Başlangıç tarihini güncelle.
            ilan.TarihBitis = emlak.TarihBitis; // Bitiş tarihini güncelle.
            ilan.TarihTeslim = emlak.TarihTeslim; // Teslim tarihini güncelle.
            ilan.BagceSayisi = emlak.BagceSayisi; // Bağçe sayısını güncelle.
            ilan.BahceSayisiIstenen = emlak.BahceSayisiIstenen; // İstenen bahçe sayısını güncelle.
            ilan.Taksitlimi = emlak.Taksitlimi; // Vadeli satış durumu.
            ilan.TaksitSayisi = emlak.TaksitSayisi; // Taksit sayısını güncelle.
            ilan.TaksitBedeli = emlak.TaksitBedeli; // Taksit bedelini güncelle.
            ilan.ParselSorgu = emlak.ParselSorgu; // Parsel sorgu adresi.
            ilan.IlanAciklamasi = emlak.IlanAciklamasi; // İlan açıklamasını güncelle.

            // **4. Resimleri Güncelleme**
            // Silinecek ve yeni eklenen resimler üzerinde işlem yapıyoruz.

            // 4.1. Silinecek Resimler
            // Kullanıcı tarafından işaretlenen resimleri `IsDeleted` olarak işaretliyoruz.
            var fileService = new FileService(_context); // Dosya işlemleri için yardımcı sınıf.
            await fileService.SilKayitlariIsDeletedAsync<ArsaResim>(silinecekResimler);

            // 4.2. Yeni Resimleri Yükleme
            // Yeni eklenen resimleri belirtilen dizine kaydediyoruz.
            await fileService.YukleYeniResimlerAsync<ArsaResim>(yeniResimler, ilan.IlanNo.Value, "wwwroot/emlakresimler");

            // **5. Değişiklikleri Kaydetme**
            try
            {
                // Veritabanında yapılan değişiklikleri kaydediyoruz.
                await _context.SaveChangesAsync();
                // İşlem başarılıysa, kullanıcıya başarı mesajı göster.
                TempData["UpdateEmlakDetayMessage"] = "Emlak ilanı başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                // Eğer bir hata oluşursa, hata mesajını yazdır ve kullanıcıya bilgi ver.
                TempData["UpdateEmlakDetayMessage"] = "Emlak ilanı güncellenirken bir hata oluştu." + ex.Message;
            }

            // Kullanıcıyı IlanGuncellendi sayfasına yönlendir
            return RedirectToAction("IlanGuncellendi");
        }

        public IActionResult IlanGuncellendi()
        {
            return View();
        }

        public IActionResult Kazanclar()
        {
            return PartialView("~/Views/Shared/Kullanici/_Kazanclar.cshtml");
        }

        public IActionResult EditAraba(int id)
        {

            // var kullaniciId = HttpContext.Session.GetString("KullaniciId");
            int.TryParse(HttpContext.Session.GetString("KullaniciId"), out int kullaniciIdInt);
            var arabaId = _context.Arabalar.FirstOrDefault(a => a.Id == id);
            if (kullaniciIdInt != arabaId?.KullaniciId)
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
                ArabaResimleri = a.ArabaResimleri.Where(r => r.IsDeleted == 0).ToList()
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
        public async Task<IActionResult> EditAraba(int id, ArabaViewModel model, List<IFormFile> yeniResimler, string[] silinecekResimler, string Il, string Ilce, string MahalleKoyAdi)
        {
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
                TempData["UpdateArabaDetayMessage"] = "Araç ilanı güncellenirken bir hata oluştu." + ex.Message;
            }

            return RedirectToAction("ArabaGuncellendi");//, new { id = araba.Id }
        }

        public IActionResult ArabaGuncellendi()
        {
            ViewBag.KullaniciId = HttpContext.Session.GetString("KullaniciId");
            return View();
        }

        //          - - - - - ŞİMDİLİK KAPATTIM - - - - - 
        //         public IActionResult UpdateUser(int id) 
        //         {
        //             var user = _context.Kullanicilar.Find(id);
        //             if (user == null)
        //             {
        //                 return NotFound();
        //             }
        //             return View(user);
        //         }
        //         // Kullanıcı Profilini Güncelle
        //         [HttpPost]
        // public IActionResult UpdateUser(RegisterViewModel model)
        // {
        //     var user = _context.Kullanicilar.Find(User.Identity.Name); // Kullanıcı kimliği
        //     if (user != null)
        //     {
        //         user.Ad = model.Ad;
        //         user.Soyad = model.Soyad;
        //         user.Telefon = model.Telefon;
        //         user.Email = model.Email;

        //         if (model.Sifre != null)
        //         {
        //             user.SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre);
        //         }

        //         if (model.ImgUrl != null)
        //         {
        //             // Resim güncelleme işlemi
        //         }

        //         _context.SaveChanges();
        //         return RedirectToAction("Profile");
        //     }

        //     return View(model);
        // }


        // // Yeni Kullanıcı Kaydı - GET
        // public IActionResult Register()
        // {
        //     return View();
        // }

        // // Yeni Kullanıcı Kaydı - POST
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Register(RegisterViewModel model)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var user = new Kullanici
        //         {
        //             Ad = model.Ad,
        //             Soyad = model.Soyad,
        //             Telefon = model.Telefon,
        //             SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre),
        //             Rol = "üye",
        //             FirmaUnvani = model.FirmaUnvani,
        //             Referans = model.Referans,
        //             KayitTarihi = DateTime.Now
        //         };

        //         _context.Kullanicilar.Add(user);
        //         _context.SaveChanges();
        //         return RedirectToAction("Index", "Home");
        //     }
        //     return View(model);
        // }

        // Diğer kullanıcı işlemleri (Edit, Delete) burada kalabilir.
    }
}
