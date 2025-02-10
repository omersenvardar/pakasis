// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using DBGoreWebApp.Data;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;

// namespace DBGoreWebApp.Controllers
// {
//     // [Route("[controller]")]
//     public class EmlakController : Controller
//     {
//         private readonly ILogger<ArabalarController> _logger;
//         private readonly ApplicationDbContext _context;

//         public EmlakController(ILogger<ArabalarController> logger, ApplicationDbContext context)
//         {
//             _logger = logger;
//             _context = context;
//         }

//         public IActionResult Index()
//         {
//             return View();
//         }
//         [HttpGet]
// public async Task<IActionResult> Emlak(string altkategori)
// {
//     if (string.IsNullOrEmpty(altkategori))
//     {
//         return RedirectToAction("Index", "Home"); // Alt kategori belirtilmezse ana sayfaya yönlendir
//     }

//     // Veritabanında alt kategoriye göre sorgu
//     var emlaklar = await _context.EmlakBahceler
//         .Where(e => e.Turu == altkategori && (e.Durum == 'v' || e.Durum == 'a')) // Alt kategoriye ve Durum'a göre filtreleme
//         .OrderByDescending(e => e.CreatedDate) // İlan verme tarihine göre sıralama
//         .ToListAsync();

//     ViewBag.AltKategori = altkategori; // Görünümde kategori adını göstermek için

//     return View(emlaklar);
// }


//         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//         public IActionResult Error()
//         {
//             return View("Error!");
//         }
//     }
// }