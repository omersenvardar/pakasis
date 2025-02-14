using System;
using System.IO; // Dosya işlemleri için
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment için
using Microsoft.Extensions.Hosting;

using DBGoreWebApp.Data;
using DBGoreWebApp.Models.ViewModels;
using DBGoreWebApp.Models.Entities;
using BCrypt.Net;

namespace DBGoreWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // Giriş Sayfası - GET
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcıyı veritabanında kontrol et
                var user = _context.Kullanicilar.FirstOrDefault(u =>
                    u.Email == model.EmailOrPhone || u.Telefon == model.EmailOrPhone);

                if (user == null)
                {
                    TempData["LoginMessage"] = "Bu e-posta veya telefon numarası ile kayıtlı kullanıcı bulunamadı.";
                }
                else if (!BCrypt.Net.BCrypt.Verify(model.Sifre, user.SifreHash))
                {
                    TempData["LoginMessage"] = "Şifre hatalı.";
                }
                else
                {
                    // Oturum bilgilerini sakla
                    HttpContext.Session.SetString("KullaniciAd", user.Ad);
                    HttpContext.Session.SetString("KullaniciId", user.Id.ToString());
                    HttpContext.Session.SetString("KullaniciYetki", user.Rol ?? "üye"); // Varsayılan olarak "üye" atanır
                    HttpContext.Session.SetString("KullaniciProfilResmi", user.ImgUrl ?? "/img/default-user.jpg");

                    TempData["LoginMessage"] = "Giriş başarılı.";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }


        // Çıkış İşlemi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = " Çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        // Yeni Kullanıcı Kayıt Sayfası - GET
        public IActionResult Register()
        {
            return View();
        }

        // Kullanıcı Kayıt İşlemi - POST
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Register(RegisterViewModel model)
{
    if (ModelState.IsValid)
    {
        // E-posta benzersizliğini kontrol et
        if (_context.Kullanicilar.Any(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
            return View(model);
        }

        try
        {
            // Yeni kullanıcı oluştur
            var kullanici = new Kullanici
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Telefon = model.Telefon,
                Email = model.Email,
                SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre),
                FirmaUnvani = model.FirmaUnvani,
                Referans = model.Referans,
                Rol = "üye",  // Rol burada string olarak kaydediliyor.
                Durum = 'p',
                AbonelikTipi = null,
                KayitTarihi = DateTime.Now,
                AbonelikStatusu = "aktif" // Abonelik durumu metin olarak kaydediliyor.
            };

            // Resim yükleme işlemi
            if (model.ImgUrl != null && model.ImgUrl.Length > 0)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImgUrl.FileName);
                var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", uniqueFileName);

                using (var stream = new FileStream(uploadPath, FileMode.Create, FileAccess.Write))
                {
                    model.ImgUrl.CopyTo(stream);
                }

                kullanici.ImgUrl = "/uploads/" + uniqueFileName;
            }

            // Kullanıcıyı veritabanına kaydet
            _context.Kullanicilar.Add(kullanici);
            _context.SaveChanges();

            // Kullanıcıyı otomatik olarak login yap
            HttpContext.Session.SetString("KullaniciAd", kullanici.Ad);
            HttpContext.Session.SetString("KullaniciId", kullanici.Id.ToString());
            HttpContext.Session.SetString("KullaniciYetki", kullanici.Rol);
            HttpContext.Session.SetString("KullaniciProfilResmi", kullanici.ImgUrl ?? "/img/default-user.jpg");

            TempData["MessageRegister"] = "Kaydınız başarılı! Giriş yapıldı ve ana sayfaya yönlendiriliyorsunuz.";
            return RedirectToAction("RegisterSuccess");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Bir hata oluştu: " + ex.Message);
            return View(model);
        }
    }

    TempData["MessageRegisterhata"] = "Form bilgileri eksik veya hatalı.";
    return View(model);
}

        // Kayıt başarılı sayfası
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterSuccess()
        {
            return View();
        }
        [HttpGet("Kullanici/Profil/EditProfilResimi/{id}")]
        public IActionResult EditProfilResimi(int id)
        {
            var user = _context.Kullanicilar.Find(id);
            if (user == null) return NotFound();

            return PartialView("Kullanici/Profil/_EditProfilResimiPartial", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfilResimi(int id, IFormFile ImgUrl)
        {

            // Kullanıcıyı veritabanında bul
            var user = _context.Kullanicilar.Find(id);
            if (user == null)
            {
                return NotFound();
            }


            // Resim kontrolü
            if (ImgUrl != null && ImgUrl.Length > 0)
            {

                try
                {
                    // Dosya adı ve yolu oluşturuluyor
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(ImgUrl.FileName);

                    var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", uniqueFileName);

                    // Resim dosyasını kaydet
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        ImgUrl.CopyTo(stream);
                    }

                    // Kullanıcıya yeni resim yolunu kaydet
                    user.ImgUrl = "/uploads/" + uniqueFileName;

                    // Değişiklikleri kaydet
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Profil resmi güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
                    return RedirectToAction("Edit", new { id = user.Id });
                }
            }
            else
            {
            }

            TempData["SuccessMessage"] = "Profil resmi başarıyla güncellendi.";
            return RedirectToAction("Detail", "Users", new { Id = id });
        }


        [HttpGet("Kullanici/Profil/EditKullanici/{id}")]
        public IActionResult EditKullanici(int id)
        {
            var user = _context.Kullanicilar.Find(id);
            if (user == null) return NotFound();

            return PartialView("Kullanici/Profil/_EditKullaniciPartial", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateKullanici(Kullanici model)
        {
            var user = _context.Kullanicilar.Find(model.Id);
            if (user == null) return NotFound();

            // Kullanıcı bilgilerini güncelle
            user.Ad = model.Ad;
            user.Soyad = model.Soyad;
            user.Telefon = model.Telefon;
            user.Email = model.Email;

            // Veritabanına değişiklikleri kaydet
            _context.SaveChanges();

            // TempData ile başarı mesajını ilet
            TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";

            // Users/Detail/{id} adresine yönlendir
            return RedirectToAction("Detail", "Users", new { id = model.Id });
        }

        // Şifre Güncelleme Sayfası - GET
        [HttpGet("Kullanici/Profil/EditPassword/{id}")]
        public IActionResult EditPassword(int id)
        {
            var user = _context.Kullanicilar.Find(id);
            if (user == null)
                return NotFound();

            var model = new PasswordChangeViewModel
            {
                Id = user.Id
            };

            return PartialView("Kullanici/Profil/_EditPassPartial", model);
        }

        // Şifre Güncelleme İşlemi
        [HttpPost]
        public IActionResult UpdatePassword(PasswordChangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Kullanicilar.Find(model.Id);
                if (user == null) return NotFound();

                // Mevcut şifre kontrolü
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.SifreHash))
                {
                    ModelState.AddModelError(string.Empty, "Mevcut şifre yanlış.");
                    return PartialView("Kullanici/Profil/_EditPassPartial", model);
                }

                // Yeni şifre ve onay eşleşmiyor
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError(string.Empty, "Yeni şifreler eşleşmiyor.");
                    return PartialView("Kullanici/Profil/_EditPassPartial", model);
                }

                // Yeni şifreyi hashleyerek kaydet
                user.SifreHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
                return RedirectToAction("Detail", "Users", new { id = model.Id });
            }

            return PartialView("Kullanici/Profil/_EditPassPartial", model);
        }

        // Hata Sayfası
        [HttpGet("Error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
