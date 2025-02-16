using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DBGoreWebApp.Data;
using DBGoreWebApp.Models.Entities;
using DBGoreWebApp.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için
using Microsoft.Extensions.Hosting;

namespace DBGoreWebApp.Controllers
{
    public class ArabalarController : Controller
    {
        private readonly ILogger<ArabalarController> _logger;
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostingEnvironment;

        public ArabalarController(ILogger<ArabalarController> logger, ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _context = context;
            _hostingEnvironment = hostEnvironment;
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
        public IActionResult Index(int page = 1, int pageSize = 12)
        {
            var query = _context.Arabalar
                .Include(a => a.Marka)
                .Include(a => a.Model)
                .Include(a => a.Yil)
                .Include(a => a.AdresKonumuNavigation)
                .Where(a => a.Durum == 'a' || a.Durum == 'v'); // Gereksiz verileri çekmemek için filtreleme en başta yapılmalı.

            // Toplam kayıt sayısını hesapla (gereksiz tüm arabaları çekmemek için ayrı bir sorgu)
            var totalItems = query.Count();

            // Sayfalama ve filtreleme
            var arabalar = query
                .OrderByDescending(a => a.CreatedDate) // Son ekleme tarihine göre sırala
                .Skip((page - 1) * pageSize) // Sayfa başına belirtilen kadar atla
                .Take(pageSize) // Sayfa başına belirlenen kadar al
                .ToList();

            // Sadece bu sayfadaki arabaların ID'lerini çekiyoruz
            var arabaIds = arabalar.Select(a => a.Id).ToList();
            var versiyonIds = arabalar.Select(a => a.VersiyonID).Distinct().ToList();

            // Resim URL'lerini verimli şekilde alıyoruz (Sadece bu sayfadaki arabalar için)
            ViewBag.ArabaResimUrls = _context.ArabaResimleri
                .Where(r => arabaIds.Contains(r.ArabaId))
                .GroupBy(r => r.ArabaId)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault().ResimArabaUrl ?? "/img/default-car.jpg");

            // Versiyon Adlarını sadece ilgili ID'ler için çekiyoruz (Tüm tabloyu değil)
            ViewBag.ArabaVersiyonlar = _context.AracYilVersiyons
                .Where(v => versiyonIds.Contains(v.VersiyonID))
                .ToDictionary(v => v.VersiyonID, v => v.VersiyonAdi);

            // Toplam sayfa sayısı ve mevcut sayfa numarası
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;

            return View(arabalar);
        }



        // Detay Görüntüleme (Detail)
        public IActionResult Detail(int id)
        {
            var araba = _context.Arabalar.FirstOrDefault(a => a.Id == id);
            if (araba == null) return NotFound();

            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == araba.KullaniciId);
            var resimler = _context.ArabaResimleri.Where(r => r.ArabaId == araba.Id).ToList();
            var versiyon = _context.AracYilVersiyons.FirstOrDefault(v => v.VersiyonID == araba.VersiyonID);
            var markaAdi = _context.AracMarkalaris.FirstOrDefault(m => m.MarkaID == araba.MarkaID)?.Marka;
            var modelAdi = _context.AracModelListesis.FirstOrDefault(m => m.ModelID == araba.ModelID)?.Model;
            var yilAdi = _context.AracModelYillaris.FirstOrDefault(y => y.YilID == araba.YilID)?.Yil.ToString();

            var model = new ArabaViewModel
            {
                ArabaDetay = araba,
                KullaniciDetay = kullanici,
                ArabaResimler = resimler,
                AracYilVersiyon = versiyon,
                MarkaAdi = markaAdi,
                ModelAdi = modelAdi,
                YilAdi = yilAdi
            };

            return View(model);
        }
        [HttpGet]
        public JsonResult GetModels(int markaId)
        {
            var models = _context.AracModelListesis.Where(m => m.MarkaID == markaId).ToList();
            return Json(models);
        }

        [HttpGet]
        public JsonResult GetYears(int modelId)
        {
            var years = _context.AracModelYillaris.Where(y => y.ModelID == modelId).ToList();
            return Json(years);
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
        public IActionResult Create()
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            // Adres Gruplarını Getir
            var adresGruplari = GetAdresGruplari();

            // Kullanıcı ID'sini al ve doğrula
            var kullaniciId = GetKullaniciIdFromSession();
            if (kullaniciId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcı detaylarını ve diğer verileri doldur
            var viewModel = new ArabaViewModel
            {
                KullaniciDetay = _context.Kullanicilar.FirstOrDefault(v => v.Id == kullaniciId),
                Adresler = _context.Adresler.ToList(),
                AracMarkalaris = _context.AracMarkalaris.ToList(),
                AracModelListesis = new List<AracModelListesi>(), // Varsayılan boş liste
                AracModelYillaris = new List<AracModelYillari>(), // Varsayılan boş liste
                AracYilVersiyons = new List<AracYilVersiyon>(),   // Varsayılan boş liste
            };

            ViewBag.AdresGruplari = adresGruplari;
            return View(viewModel);
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

        // Kullanıcı ID'sini Session'dan Al
        private int? GetKullaniciIdFromSession()
        {

            var kullaniciIdString = HttpContext.Session.GetString("KullaniciId");

            if (!string.IsNullOrEmpty(kullaniciIdString) && int.TryParse(kullaniciIdString, out int kullaniciId))
            {
                return kullaniciId;
            }

            return null; // Geçersiz veya oturum açılmamış
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ArabaViewModel model, List<IFormFile> resimler)
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }


            // Kullanıcı ID'sini al ve kontrol et
            string? kullaniciIdStr = HttpContext.Session.GetString("KullaniciId");
            if (string.IsNullOrEmpty(kullaniciIdStr) || !int.TryParse(kullaniciIdStr, out int kullaniciId))
            {
                ModelState.AddModelError("", "Oturum açılmamış. Lütfen giriş yapınız.");
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

            try
            {
                // Araba modelini dolduralım
                var araba = new Araba
                {

                    KullaniciId = kullaniciId,
                    MarkaID = model.MarkaID,
                    ModelID = model.ModelID,
                    YilID = model.YilID,
                    VersiyonID = model.VersiyonID,
                    AdresKonumu = model.AdresKonumu,
                    VersiyonAdi = model.VersiyonAdi,
                    Ozellikleri = model.Ozellikleri,
                    Fiyat = model.Fiyat,
                    Durum = 'p' // Varsayılan durum (Pending)

                };
                // Araba bilgilerini veritabanına ekleyelim
                _context.Arabalar.Add(araba);
                _context.SaveChanges();

                // Resim işlemleri
                if (resimler != null && resimler.Count > 0)
                {
                    //string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resimaraba");
                    // **Doğru dosya yolu (Plesk İçin)**
                    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "resimaraba");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder); // Klasör yoksa oluştur
                    }

                    foreach (var resim in resimler)
                    {
                        if (resim.Length > 0)
                        {
                            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(resim.FileName);
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                resim.CopyTo(fileStream);
                            }

                            // **Veritabanına doğru URL formatında kaydet**
                            var arabaResim = new ArabaResim
                            {
                                ArabaId = araba.Id,
                                ResimArabaUrl = $"/resimaraba/{uniqueFileName}", // **Burada başında `.` olmamalı**
                                ResimAdi = uniqueFileName
                            };
                            _context.ArabaResimleri.Add(arabaResim);
                        }
                    }

                    _context.SaveChanges();

                }
                return RedirectToAction("Ilanverildi", "EmlakBahce");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyiniz." + ex.Message);
                return View(model);
            }
        }

        // Güncelleme (Edit - GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var araba = _context.Arabalar.FirstOrDefault(a => a.Id == id);
            if (araba == null) return NotFound();
            return View(araba);
        }

        // Güncelleme (Edit - POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Araba araba)
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (ModelState.IsValid)
            {
                _context.Arabalar.Update(araba);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(araba);
        }

        // Silme (Delete - GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var araba = _context.Arabalar.FirstOrDefault(a => a.Id == id);
            if (araba == null) return NotFound();
            return View(araba);
        }

        // Silme (Delete - POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var yetki = HttpContext.Session.GetString("KullaniciYetki");
            if (yetki != "üye" && yetki != "admin")
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var araba = _context.Arabalar.FirstOrDefault(a => a.Id == id);
            if (araba == null) return NotFound();

            _context.Arabalar.Remove(araba);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


    }
}
