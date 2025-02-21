using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DBGoreWebApp.Models;
using DBGoreWebApp.Data;
using Microsoft.EntityFrameworkCore;
using DBGoreWebApp.Models.ViewModels;
using DBGoreWebApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace DBGoreWebApp.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
  private readonly ILogger<HomeController> _logger;
  private readonly ApplicationDbContext _context;
  public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
  {
    _context = context;
    _logger = logger;
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


  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> Emlak(int page = 1)
  {
    int pageSize = 9; // Sayfa başına gösterilecek ilan sayısı
    var totalItems = _context.Arsa.Count(); // Toplam ilan sayısı
    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize); // Sayfa sayısını hesapla

    var arsalar = _context.Arsa
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var kullanicilar = await _context.Kullanicilar.ToListAsync();
    var emlakBahcelers = await _context.EmlakBahceler.ToListAsync();
#pragma warning disable CS8604 // Possible null reference argument.
    var arsaResimler = await _context.ArsaResimleri.ToListAsync();
#pragma warning restore CS8604 // Possible null reference argument.
    var adresler = await _context.Adresler.ToListAsync();

    // İl, ilçe ve mahalleleri gruplandır
    var groupedAdresler = adresler
        .GroupBy(a => a.Il)
        .ToDictionary(
            g => g.Key,
            g => g.GroupBy(a => a.Ilce)
                  .ToDictionary(
                      ilceGroup => ilceGroup.Key,
                      ilceGroup => ilceGroup.Select(a => a.MahalleKoyAdi).ToList()
                  )
        );

    ViewBag.AdresGruplari = groupedAdresler;

    var model = new CompositeViewModel()
    {
      EmlakBahceler = emlakBahcelers,
      Kullanicilar = kullanicilar,
      ArsaResimler = arsaResimler,
      Adresler = adresler,
      CurrentPage = page,
      TotalPages = totalPages
    };

    return View(model);
  }


  [HttpGet]
  public async Task<IActionResult> FilterEmlak(string il, string ilce, string mahalle)
  {
    //   string AdresKonumu = il + ", " + ilce + ", " + mahalle;
    //     var filteredEmlakBahceler = await _context.EmlakBahceler
    //         .Where(e => e.AdresKonumu == AdresKonumu)
    //         .ToListAsync();

    //     var model = new CompositeViewModel()
    //     {
    //         EmlakBahceler = filteredEmlakBahceler,
    //         Kullanicilar = await _context.Kullanicilar.ToListAsync(),
    //         ArsaResimler = await _context.ArsaResimleri.ToListAsync()
    //     };

    return PartialView("Emlak/_EmlakListPartial");
  }
  [HttpGet]
  public async Task<IActionResult> FilterAdres()
  {
    var adresler = await _context.Adresler
    .GroupBy(a => a.Il) // İl bazında grupla
    .Select(g => g.FirstOrDefault()) // Her il için ilk adresi seç
    .ToListAsync();

    return PartialView("Emlak/_EmlakAdresIlPartial", adresler);
  }
  // public IActionResult Index()
  // {
  //     var arabalar = _context.Arabalar
  //         .Where(a => a.Durum == 'a' || a.Durum == 'v')
  //         .OrderByDescending(a => a.CreatedDate)
  //         .ToList();

  //     var emlakBahceler = _context.EmlakBahceler
  //         .Where(e => e.Durum == 'a' || e.Durum == 'v')
  //         .OrderByDescending(e => e.CreatedDate)
  //         .ToList();

  //     var viewModel = new IlanViewModel
  //     {
  //         Arabalar = arabalar,
  //         EmlakBahceler = emlakBahceler
  //     };

  //     return View(viewModel);
  // }


  [HttpGet]
  public async Task<IActionResult> Index(int page = 1, int pageSize = 12)
  {
    int IlanPageSize = pageSize;
    var settings = _context.AdminSettings
        .Where(s => s.SettingKey != null && s.SettingValue != null) // Null kontrolü
        .ToDictionary(s => s.SettingKey!, s => s.SettingValue!);

    ViewBag.IlanKolonSayisi = settings.ContainsKey("hpageKolonSayisi") ? settings["hpageKolonSayisi"] : "width: 40%; margin-top: 20px;";

    ViewBag.homeTitle = settings.ContainsKey("sitetitle") ? settings["sitetitle"] : "Pakasis aracılığı ile güvenle alloooo, zahmetsizce sat.";
    ViewBag.HomeMessage = settings.ContainsKey("siteBaslik") ? settings["siteBaslik"] : "Pakasis aracılığı ile güvenle alloooo, zahmetsizce sat.";

    // Emlak ilanlarını al
#pragma warning disable CS8604 // Possible null reference argument.
    var emlaklar = await _context.EmlakBahceler
        .Where(e => e.Durum == 'v')
        .Select(e => new IlanViewModel
        {
          Id = e.Id,
          IlanAdi = e.IlanAdi,
          Adres = e.AdresKonumuNavigation != null
                ? $"{e.AdresKonumuNavigation.Il}, {e.AdresKonumuNavigation.Ilce}"
                : "Adres bilgisi yok",
          YuzOlcum = e.YuzOlcum,
          Turu = e.Turu,
          Fiyat = e.FiyatSatis,
          ResimUrl = _context.ArsaResimleri
                .Where(r => r.ArsaId == e.IlanNo) // ArsaId ile IlanNo eşleştirilir
                .Select(r => r.ResimArsaUrl)
                .FirstOrDefault() ?? "/img/default-emlak.jpg",
          CreatedDate = e.CreatedDate,
          IlanTuru = "Emlak"
        })
        .ToListAsync();
#pragma warning restore CS8604 // Possible null reference argument.
    // Araba ilanlarını al
    var arabalar = await (from a in _context.Arabalar
                          join am in _context.AracMarkalaris on a.MarkaID equals am.MarkaID into amGroup
                          from am in amGroup.DefaultIfEmpty()
                          where a.Durum == 'v'
                          select new IlanViewModel
                          {
                            Id = a.Id,
                            IlanAdi = a.VersiyonAdi,
                            Adres = a.AdresKonumuNavigation != null
                                  ? $"{a.AdresKonumuNavigation.Il}, {a.AdresKonumuNavigation.Ilce}"
                                  : "Adres bilgisi yok",
                            Marka = am != null ? am.Marka : "Marka bilgisi yok",
                            MarkaID = a.MarkaID,
                            ModelID = a.ModelID,
                            Fiyat = a.Fiyat,
                            ResimUrl = a.ArabaResimleri.FirstOrDefault() != null
                                  ? a.ArabaResimleri.FirstOrDefault().ResimArabaUrl
                                  : "/img/default-car.jpg",
                            CreatedDate = a.CreatedDate,
                            IlanTuru = "Araba"
                          })
                          .ToListAsync();

    // Her bir arabaya ait model ve yıl bilgilerini almak
    foreach (var araba in arabalar)
    {
      araba.AracModelListesis = await _context.AracModelListesis
                                            .Where(x => x.ModelID == araba.ModelID)
                                            .ToListAsync();
      araba.aracModelYillaris = await _context.AracModelYillaris
                                            .Where(x => x.ModelID == araba.ModelID)
                                            .ToListAsync();
      araba.Model = araba.AracModelListesis.FirstOrDefault()?.Model ?? "Model bilgisi yok";
      araba.Yil = araba.aracModelYillaris.FirstOrDefault()?.Yil.ToString() ?? "Yıl bilgisi yok";
    }
    // İlanları birleştir ve sırala
    var combinedList = emlaklar.Concat(arabalar)
        .OrderByDescending(i => i.CreatedDate)
        .ToList();

    // Toplam ilan sayısını ve sayfalama bilgilerini hesapla
    int totalItems = combinedList.Count;
    var pagedList = combinedList
        .Skip((page - 1) * IlanPageSize)
        .Take(IlanPageSize)
        .ToList();

    ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / IlanPageSize);
    ViewBag.CurrentPage = page;

    return View(pagedList);
  }

  public string GetTaksitText(EmlakBahce emlak)
  {
    if (emlak.Taksitlimi == "E")
    {
      // String değerleri sayıya dönüştürme
      bool isValidBedel = int.TryParse(emlak.TaksitBedeli, out int taksitBedeli);
      bool isValidSayisi = int.TryParse(emlak.TaksitSayisi, out int taksitSayisi);

      if (isValidBedel && isValidSayisi)
      {
        int toplamFiyat = taksitBedeli * taksitSayisi;
        return $"Bu bahçeyi {toplamFiyat:N0} ₺ fiyata, {emlak.Kapora} ₺ peşinat ve {taksitBedeli:N0} ₺'den başlayan taksitlerle alabilirsiniz.";
      }
    }
    return string.Empty;
  }






  [HttpGet]
  public new IActionResult NotFound()
{
    return View("NotFoundView");
}


  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}

