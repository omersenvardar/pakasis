using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DBGoreWebApp.Models.Entities;
using DBGoreWebApp.Models.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DBGoreWebApp.Controllers
{
    public class EmlakBahceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public EmlakBahceController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        private void LogHata(Exception ex)
        {
            string logPath = Path.Combine(_hostingEnvironment.WebRootPath, "logs", "error_log.txt");

            // Eğer logs klasörü yoksa oluştur
            if (!Directory.Exists(Path.GetDirectoryName(logPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            }

            // Hata mesajını dosyaya ekle
            System.IO.File.AppendAllText(logPath,
                $"{DateTime.Now}: Hata oluştu - {ex.Message}\nStackTrace: {ex.StackTrace}\n\n");
        }
        public async Task<IActionResult> GetCitiesWithCount()
        {
            var cities = await _context.Arsa
                .GroupBy(x => x.Il)
                .Select(g => new { name = g.Key, count = g.Count() })
                .ToListAsync();
            return Json(cities);
        }

        public async Task<IActionResult> GetDistrictsWithCount(string city)
        {
            var districts = await _context.Arsa
                .Where(x => x.Il == city)
                .GroupBy(x => x.Ilce)
                .Select(g => new { name = g.Key, count = g.Count() })
                .ToListAsync();
            return Json(districts);
        }


        [HttpGet]
        public async Task<IActionResult> GetDistricts(string city)
        {
            var districts = await _context.Adresler
                .Where(a => a.Il == city)
                .Select(a => a.Ilce)
                .Distinct()
                .ToListAsync();
            return Json(districts);
        }


        [HttpGet]
        public async Task<IActionResult> GetMahalleler(string district)
        {
            var mahalleler = await _context.Adresler
                .Where(a => a.Ilce == district)
                .Select(a => a.MahalleKoyAdi)
                .Distinct()
                .ToListAsync();

            return Json(mahalleler);
        }

        // İl seçildiğinde ilçeleri getir
        [HttpGet]
        public IActionResult Ilanver()
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var adresler = _context.Adresler.ToList(); // Adres bilgilerini model ile sağlıyoruz
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
            ViewBag.AdresGruplari = adresGruplari;
            var model = new EmlakIlanverViewModel
            {
                Adresler = adresler

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ilanver(EmlakIlanverViewModel model)
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }


            // Session'dan KullaniciId'yi al 
            string kullaniciIdStr = HttpContext.Session.GetString("KullaniciId");
            if (!string.IsNullOrEmpty(kullaniciIdStr) && int.TryParse(kullaniciIdStr, out int kullaniciIdInt))
            {
                model.KullaniciId = kullaniciIdInt; // Kullanıcı Id'yi modele ata
            }
            else
            {
                ModelState.AddModelError("", "Oturum süresi doldu. Lütfen tekrar giriş yapın.");
                model.Adresler = _context.Adresler.ToList();
                return View(model);
            }

            // İl, İlçe ve Mahalle seçimini doğrula
            var adres = _context.Adresler.FirstOrDefault(a =>
                a.Il == model.Il && a.Ilce == model.Ilce && a.MahalleKoyAdi == model.MahalleKoyAdi);

            if (adres == null)
            {
                ModelState.AddModelError("", "Geçersiz adres seçimi. Lütfen adres bilgilerini kontrol edin.");
                model.Adresler = _context.Adresler.ToList();
                return View(model);
            }

            model.AdresKonumu = adres.Id;

            if (!ModelState.IsValid)
            {
                model.Adresler = _context.Adresler.ToList();
                return View(model);
            }

            var yeniIlanNo = (_context.EmlakBahceler.Max(e => (int?)e.IlanNo) ?? 100) + 1;

            var emlak = new EmlakBahce
            {
                IlanNo = yeniIlanNo,
                IlanAdi = model.IlanAdi,
                KullaniciId = model.KullaniciId.Value,
                AdresKonumu = model.AdresKonumu,
                Ada = model.Ada,
                Parsel = model.Parsel,
                YuzOlcum = model.YuzOlcum,
                Hisse = model.Hisse,
                Turu = model.Turu,
                BagceSayisi = model.BagceSayisi,
                BahceSayisiIstenen = model.BahceSayisiSatilan,
                TarihBas = model.TarihBas,
                TarihBitis = model.TarihBitis,
                TarihTeslim = model.TarihTeslim,
                FiyatSatis = model.FiyatSatis,
                FiyatPesin = model.FiyatPesin,
                Kapora = model.Kapora,
                Taksitlimi = model.Taksitlimi,
                TaksitBedeli = model.TaksitBedeli,
                TaksitSayisi = model.TaksitSayisi,
                IlanAciklamasi = model.IlanAciklamasi,
                ParselSorgu = model.ParselSorgu,
                Durum = 'p'
            };

            _context.EmlakBahceler.Add(emlak);
            await _context.SaveChangesAsync();

            if (model.Resimler != null && model.Resimler.Count > 0)
            {
                foreach (var resim in model.Resimler)
                {
                    if (resim.Length > 0)
                    {
                        var dosyaAdi = $"{Guid.NewGuid()}{Path.GetExtension(resim.FileName)}";
                        var dosyaYolu = Path.Combine("./wwwroot/emlakresimler", dosyaAdi);

                        using (var stream = new FileStream(dosyaYolu, FileMode.Create))
                        {
                            await resim.CopyToAsync(stream);
                        }

                        var arsaResim = new ArsaResim
                        {
                            ArsaId = yeniIlanNo,
                            ResimArsaUrl = $"/emlakresimler/{dosyaAdi}",
                            ResimAdi = dosyaAdi
                        };

                        _context.ArsaResimleri.Add(arsaResim);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "İlan başarıyla kaydedildi.";
            return RedirectToAction("Ilanverildi", "EmlakBahce");
        }
        public IActionResult Ilanverildi()
        {
            return View();
        }


        public async Task<IActionResult> Detail(int id)
        {
            var emlak = await _context.EmlakBahceler.FirstOrDefaultAsync(e => e.Id == id);
            if (emlak == null)
            {
                return NotFound(); // Eğer emlak bulunamazsa
            }

            var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Id == emlak.KullaniciId);
            if (kullanici == null)
            {
                return NotFound(); // Eğer kullanıcı bulunamazsa
            }

            // Adres bilgilerini getir
            var adres = await _context.Adresler.FirstOrDefaultAsync(a => a.Id == emlak.AdresKonumu);

#pragma warning disable CS8604 // Possible null reference argument.
            var resimler = await _context.ArsaResimleri.Where(r => r.ArsaId == emlak.IlanNo).ToListAsync();
#pragma warning restore CS8604 // Possible null reference argument.

            var model = new CompositeViewModel()
            {
                EmlakDetay = emlak,
                KullaniciDetay = kullanici,
                ArsaResimler = resimler,
                AdresDetay = adres // Adres bilgilerini modele ekle
            };

            return View(model); // Görünümde `model` nesnesini geçiriyoruz
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            // İlgili ilanı veritabanından getir
            var emlak = _context.EmlakBahceler.FirstOrDefault(e => e.Id == id);
            if (emlak == null)
            {
                return NotFound();
            }

            // Adresleri ViewBag ile gönder
            ViewBag.Adresler = new SelectList(_context.Adresler, "Id", "FullAddress");

            return View(emlak);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateEmlak(EmlakBahce model)
        {
            if (ModelState.IsValid)
            {
                var emlak = _context.EmlakBahceler.FirstOrDefault(e => e.Id == model.Id);
                if (emlak != null)
                {
                    emlak.IlanAdi = model.IlanAdi;
                    emlak.AdresKonumu = model.AdresKonumu;
                    emlak.FiyatSatis = model.FiyatSatis;
                    emlak.Turu = model.Turu;
                    emlak.TarihBas = model.TarihBas;
                    emlak.TarihBitis = model.TarihBitis;
                    emlak.IlanAciklamasi = model.IlanAciklamasi;

                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Emlak başarıyla güncellendi.";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ErrorMessage"] = "Güncelleme sırasında bir hata oluştu.";
            }

            // Hatalı form verileri varsa, tekrar adresleri gönderin
            ViewBag.Adresler = new SelectList(_context.Adresler, "Id", "FullAddress");
            return View("Edit", model);
        }

        public async Task<ActionResult> Emlaklar(string kategoriad, string altkategori)
        {
            if (string.IsNullOrEmpty(kategoriad) || string.IsNullOrEmpty(altkategori))
            {
                return RedirectToAction("Index", "Home");
            }
            var settings = _context.AdminSettings
                .Where(s => s.SettingKey != null && s.SettingValue != null) // Null kontrolü
                .ToDictionary(s => s.SettingKey!, s => s.SettingValue!);

            ViewBag.emlaklarKolonSayisi = settings.ContainsKey("emlaklarKolonSayisi") ? settings["emlaklarKolonSayisi"] : "width: 40%; margin-top: 20px;";

            // Emlak ve ilişkili bilgileri getir
            var emlaklar = await _context.EmlakBahceler
                .Include(e => e.AdresKonumuNavigation) // Adres bilgilerini dahil et
                .Where(e => e.Turu == altkategori && (e.Durum == 'v' || e.Durum == 'a')) // Filtreleme
                .OrderByDescending(e => e.CreatedDate) // Tarihe göre sıralama
                .ToListAsync();

            // Resim URL'lerini hazırlama
            var resimUrls = emlaklar.ToDictionary(
                e => e.Id,
                e => _context.ArsaResimleri
                    .Where(r => r.ArsaId == e.IlanNo) // ArsaResimleri'nde ArsaId ile IlanNo eşleştiriliyor
                    .Select(r => r.ResimArsaUrl)
                    .FirstOrDefault() ?? "/img/default-emlak.jpg" // İlk eşleşen resim veya varsayılan resim
            );

            ViewBag.ResimUrls = resimUrls; // Resim URL'lerini ViewBag ile aktar
            ViewBag.KategoriAd = kategoriad;
            ViewBag.AltKategori = altkategori;

            return View(emlaklar);
        }



        //[HttpGet("{altkategori?}")]
        public IActionResult Arsalar(string altkategori)
        {
            if (string.IsNullOrEmpty(altkategori))
            {
                return RedirectToAction("Index", "Home");
            }

            // Alt kategoriye göre işlem yapın
            var emlaklar = _context.EmlakBahceler
                .Where(e => e.Turu == altkategori && (e.Durum == 'v' || e.Durum == 'a'))
                .OrderByDescending(e => e.CreatedDate)
                .ToList();

            return View(emlaklar);
        }

    }
}
