using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using DBGoreWebApp.Models;
using DBGoreWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DBGoreWebApp.Controllers
{
    //[Route("[controller]")]
    public class YedekController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public YedekController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // public async Task<IActionResult> GetCitiesWithCount()
        // {
        //     var cities = await _context.Arsa
        //         .GroupBy(x => x.Il)
        //         .Select(g => new { name = g.Key, count = g.Count() })
        //         .ToListAsync();
        //     return Json(cities);
        // }

        // public async Task<IActionResult> GetDistrictsWithCount(string city)
        // {
        //     var districts = await _context.Arsa
        //         .Where(x => x.Il == city)
        //         .GroupBy(x => x.Ilce)
        //         .Select(g => new { name = g.Key, count = g.Count() })
        //         .ToListAsync();
        //     return Json(districts);
        // }


        // [HttpGet]
        // public async Task<IActionResult> GetDistricts(string city)
        // {
        //     var districts = await _context.Adresler
        //         .Where(a => a.Il == city)
        //         .Select(a => a.Ilce)
        //         .Distinct()
        //         .ToListAsync();
        //     return Json(districts);
        // }


        // [HttpGet]
        // public async Task<IActionResult> GetMahalleler(string district)
        // {
        //     var mahalleler = await _context.Adresler
        //         .Where(a => a.Ilce == district)
        //         .Select(a => a.Mahalle)
        //         .Distinct()
        //         .ToListAsync();

        //     return Json(mahalleler);
        // }


        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> Emlak(int page = 1)
        // {
        //     int pageSize = 9; // Sayfa başına gösterilecek ilan sayısı
        //     var totalItems = _context.Arsa.Count(); // Toplam ilan sayısı
        //     var totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Sayfa sayısını hesapla

        //     var arsalar = _context.Arsa
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .ToList();

        //     var kullanicilar = await _context.Kullanicilar.ToListAsync();
        //     var emlakBahcelers = await _context.EmlakBahceler.ToListAsync();
        //     var arsaResimler = await _context.ArsaResimleri.ToListAsync();
        //     var adresler = await _context.Adresler.ToListAsync();

        //     // İl, ilçe ve mahalleleri gruplandır
        //     var groupedAdresler = adresler
        //         .GroupBy(a => a.Il)
        //         .ToDictionary(
        //             g => g.Key,
        //             g => g.GroupBy(a => a.Ilce)
        //                   .ToDictionary(
        //                       ilceGroup => ilceGroup.Key,
        //                       ilceGroup => ilceGroup.Select(a => a.Mahalle).ToList()
        //                   )
        //         );

        //     ViewBag.AdresGruplari = groupedAdresler;

        //     var model = new CompositeViewModel()
        //     {
        //         EmlakBahceler = emlakBahcelers,
        //         Kullanicilar = kullanicilar,
        //         ArsaResimler = arsaResimler,
        //         Adresler = adresler,
        //         CurrentPage = page,
        //         TotalPages = totalPages
        //     };

        //     return View(model);
        // }


        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> FilterEmlak(string il, string ilce, string mahalle)
        // {
        //     //   string AdresKonumu = il + ", " + ilce + ", " + mahalle;
        //     //     var filteredEmlakBahceler = await _context.EmlakBahceler
        //     //         .Where(e => e.AdresKonumu == AdresKonumu)
        //     //         .ToListAsync();

        //     //     var model = new CompositeViewModel()
        //     //     {
        //     //         EmlakBahceler = filteredEmlakBahceler,
        //     //         Kullanicilar = await _context.Kullanicilar.ToListAsync(),
        //     //         ArsaResimler = await _context.ArsaResimleri.ToListAsync()
        //     //     };

        //     return PartialView("Emlak/_EmlakListPartial");
        // }
        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> FilterAdres()
        // {
        //     var adresler = await _context.Adresler
        //     .GroupBy(a => a.Il) // İl bazında grupla
        //     .Select(g => g.FirstOrDefault()) // Her il için ilk adresi seç
        //     .ToListAsync();

        //     return PartialView("Emlak/_EmlakAdresIlPartial", adresler);
        // }


        // [HttpGet]
        // [AllowAnonymous]
        // public async Task<IActionResult> Index(/*int page = 1*/)
        // {
        //     // int pageSize = 6; // Sayfa başına gösterilecek ilan sayısı
        //     // var totalItems = _context.Arsa.Count(); // Toplam ilan sayısı
        //     // var totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Sayfa sayısını hesapla

        //     // var arsalar = _context.Arsa
        //     //     .Skip((page - 1) * pageSize)
        //     //     .Take(pageSize)
        //     //     .ToList();

        //     string? kullaniciAd = HttpContext.Session.GetString("KullaniciAd");
        //     string? kullaniciId = HttpContext.Session.GetString("KullaniciId");
        //     string? kullaniciYetki = HttpContext.Session.GetString("KullaniciYetki");

        //     var areas = await _context.Arsa.ToListAsync();
        //     var adresler = await _context.Adresler.ToListAsync();


        //     var uniqueIller = adresler
        //         .GroupBy(a => a.Il)
        //         .Select(g => g.First())
        //         .ToList();

        //     var uniqueIlceler = adresler
        //         .GroupBy(a => new { a.Il, a.Ilce })
        //         .Select(g => g.First())
        //         .ToList();

        //     var uniqueMahalleler = adresler
        //         .GroupBy(a => new { a.Ilce, a.Mahalle })
        //         .Select(g => g.First())
        //         .ToList();

        //     List<string> iller = areas
        //         .Select(x => x.Il)
        //         .Where(il => il != null)
        //         .Distinct()
        //         .Cast<string>()
        //         .ToList();

        //     var cityCounts = areas
        //         .GroupBy(x => x.Il)
        //         .Select(g => new CityCount { Il = g.Key, Count = g.Count() })
        //         .ToList();

        //     var districtCounts = areas
        //         .GroupBy(x => x.Ilce)
        //         .Select(g => new DistrictCount { Ilce = g.Key, Count = g.Count() })
        //         .ToList();

        //     var mahalleCounts = areas
        //         .GroupBy(x => x.Mahalle)
        //         .Select(g => new MahalleCount { Mahalle = g.Key, Count = g.Count() })
        //         .ToList();

        //     var konutCounts = areas
        //         .GroupBy(x => x.KonutTuru)
        //         .Select(g => new KonutTuruCount { KonutTuru = g.Key, Count = g.Count() })
        //         .ToList();

        //     var tipiCounts = areas
        //         .GroupBy(x => x.Tip)
        //         .Select(g => new TipiCount { Tip = g.Key, Count = g.Count() })
        //         .ToList();

        //     var model = new ArsaViewModel()
        //     {
        //         // Arsalar = arsalar,
        //         Iller = iller,
        //         Ilceler = areas.Select(x => x.Ilce).Where(ilce => ilce != null).Distinct().Cast<string>().ToList(),
        //         CityCounts = cityCounts,
        //         DistrictCounts = districtCounts,
        //         MahalleCounts = mahalleCounts,
        //         KonutTuruCounts = konutCounts,
        //         TipiCounts = tipiCounts,
        //         KullaniciAd = kullaniciAd,
        //         KullaniciId = kullaniciId,
        //         Adresler = adresler,
        //         UniqueIller = uniqueIller,
        //         UniqueIlceler = uniqueIlceler,
        //         UniqueMahalleler = uniqueMahalleler,
        //         //CurrentPage = page,
        //         //TotalPages = totalPages,
        //         LoginMessage = TempData["LoginMessage"] as string
        //     };

        //     return View(model);
        // }

        // [HttpGet]
        // public IActionResult Privacy()
        // {
        //     return View();
        // }

        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        // public IActionResult Error()
        // {
        //     return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        // }
    }
}