using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
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

        private void LogHata(Exception ex)
        {
            string logPath = Path.Combine(_hostingEnvironment.WebRootPath, "logs", "error_log.txt");

            if (!Directory.Exists(Path.GetDirectoryName(logPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            }

            System.IO.File.AppendAllText(logPath,
                $"{DateTime.Now}: Hata oluştu - {ex.Message}\nStackTrace: {ex.StackTrace}\n\n");
        }

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
        public IActionResult Login(LoginViewModel model, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Kullanicilar
                    .FirstOrDefault(u => u.Email == model.EmailOrPhone || u.Telefon == model.EmailOrPhone);

                if (user == null)
                {
                    TempData["LoginMessage"] = "Bu e-posta veya telefon numarası ile kayıtlı kullanıcı bulunamadı.";
                    return View(model);
                }
                else if (!BCrypt.Net.BCrypt.Verify(model.Sifre, user.SifreHash))
                {
                    TempData["LoginMessage"] = "Şifre hatalı.";
                    return View(model);
                }

                // **SESSION KAYIT**
                HttpContext.Session.SetString("KullaniciAd", user.Ad);
                HttpContext.Session.SetString("KullaniciId", user.Id.ToString());
                HttpContext.Session.SetString("KullaniciYetki", user.Rol ?? "üye");
                HttpContext.Session.SetString("KullaniciProfilResmi", user.ImgUrl ?? "/img/default-user.jpg");

                // **COOKIE KAYIT (Eğer 'Beni Hatırla' Seçildi ise)**
                if (rememberMe)
                {
                    CookieOptions cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30),
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict
                    };
                    if (!string.IsNullOrEmpty(user.Id.ToString()) || !string.IsNullOrEmpty(user.Rol) || !string.IsNullOrEmpty(user.Ad) || !string.IsNullOrEmpty(user.ImgUrl))
                    {
                        Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
                        Response.Cookies.Append("UserRole", user.Rol, cookieOptions);
                        Response.Cookies.Append("UserName", user.Ad, cookieOptions);
                        Response.Cookies.Append("UserProfilePic", user.ImgUrl ?? "/img/default-user.jpg", cookieOptions);
                    }
                }

                TempData["LoginMessage"] = "Giriş başarılı.";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserId");
            Response.Cookies.Delete("UserRole");
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("UserProfilePic");

            TempData["LogoutMessage"] = "Çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Kullanicilar.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                    return View(model);
                }

                try
                {
                    var kullanici = new Kullanici
                    {
                        Ad = model.Ad,
                        Soyad = model.Soyad,
                        Telefon = model.Telefon,
                        Email = model.Email,
                        SifreHash = BCrypt.Net.BCrypt.HashPassword(model.Sifre),
                        FirmaUnvani = model.FirmaUnvani,
                        Referans = model.Referans,
                        Rol = "üye",
                        Durum = 'p',
                        AbonelikTipi = null,
                        KayitTarihi = DateTime.Now,
                        AbonelikStatusu = "aktif"
                    };

                    _context.Kullanicilar.Add(kullanici);
                    _context.SaveChanges();

                    // **SESSION KAYIT**
                    HttpContext.Session.SetString("KullaniciAd", kullanici.Ad);
                    HttpContext.Session.SetString("KullaniciId", kullanici.Id.ToString());
                    HttpContext.Session.SetString("KullaniciYetki", kullanici.Rol);

                    TempData["MessageRegister"] = "Kaydınız başarılı!";
                    return RedirectToAction("RegisterSuccess");
                }
                catch (Exception ex)
                {
                    TempData["MessageRegisterHata"] = "Kayıt sırasında bir hata oluştu. " + ex.Message;
                    LogHata(ex);
                    return View(model);
                }
            }

            TempData["MessageRegisterHata"] = "Form bilgileri eksik veya hatalı.";
            return View(model);
        }

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
            Console.WriteLine("Profil resmi güncelleme işlemi başlatıldı.");
            Console.WriteLine($"Gelen Kullanıcı ID: {id}");

            // Kullanıcıyı veritabanında bul
            var user = _context.Kullanicilar.Find(id);
            if (user == null)
            {
                Console.WriteLine("Kullanıcı bulunamadı.");
                return NotFound();
            }

            Console.WriteLine($"Kullanıcı bulundu: {user.Ad} {user.Soyad}");

            // Resim kontrolü
            if (ImgUrl != null && ImgUrl.Length > 0)
            {
                Console.WriteLine("Resim yüklendi. İşleme başlandı.");

                try
                {
                    // Dosya adı ve yolu oluşturuluyor
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(ImgUrl.FileName);
                    Console.WriteLine($"Oluşturulan benzersiz dosya adı: {uniqueFileName}");

                    var uploadPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", uniqueFileName);
                    Console.WriteLine($"Yükleme yolu: {uploadPath}");

                    // Resim dosyasını kaydet
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        ImgUrl.CopyTo(stream);
                        Console.WriteLine("Resim başarıyla kaydedildi.");
                    }

                    // Kullanıcıya yeni resim yolunu kaydet
                    user.ImgUrl = "/uploads/" + uniqueFileName;
                    Console.WriteLine($"Kullanıcı için resim yolu güncellendi: {user.ImgUrl}");

                    // Değişiklikleri kaydet
                    _context.SaveChanges();
                    Console.WriteLine("Veritabanına değişiklikler kaydedildi.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Resim yükleme sırasında bir hata oluştu: {ex.Message}");
                    TempData["ErrorMessage"] = "Profil resmi güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
                    return RedirectToAction("Edit", new { id = user.Id });
                }
            }
            else
            {
                Console.WriteLine("Resim yüklenmedi. İşlem iptal edildi.");
            }

            TempData["SuccessMessage"] = "Profil resmi başarıyla güncellendi.";
            Console.WriteLine("Profil resmi güncelleme işlemi başarıyla tamamlandı.");
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
        [ValidateAntiForgeryToken]
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
                return RedirectToAction("Edit", new { id = model.Id });
            }

            return PartialView("Kullanici/Profil/_EditPassPartial", model);
        }
    }
}
