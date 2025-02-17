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

                    Response.Cookies.Append("UserId", user.Id.ToString(), cookieOptions);
                    Response.Cookies.Append("UserRole", user.Rol, cookieOptions);
                    Response.Cookies.Append("UserName", user.Ad, cookieOptions);
                    Response.Cookies.Append("UserProfilePic", user.ImgUrl ?? "/img/default-user.jpg", cookieOptions);
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
    }
}
