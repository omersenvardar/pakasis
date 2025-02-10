// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using System.Threading.Tasks;
// using DBGoreWebApp.Data;
// using Microsoft.EntityFrameworkCore;
// using DBGoreWebApp.Models;
// using DBGoreWebApp.Models.Entities;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using DBGoreWebApp.Models.ViewModels;

// namespace DBGoreWebApp.Controllers
// {
//     // [Route("[controller]")]
//     public class ArsalarController : Controller
//     {
//         private readonly ILogger<ArsalarController> _logger;
//         private readonly ApplicationDbContext _context;

//         public ArsalarController(ApplicationDbContext context, ILogger<ArsalarController> logger)
//         {
//             _context = context;
//             _logger = logger;
//         }

//         [AllowAnonymous]
//         public IActionResult Index()
//         {
//             return View();
//         }
//         public IActionResult Ilanver()
//         {
//             // var adresler = _context.Adresler.ToList();

//             // // İl, ilçe ve mahalleleri gruplandır
//             // var groupedAdresler = adresler
//             //     .GroupBy(a => a.Il)
//             //     .ToDictionary(
//             //         g => g.Key,
//             //         g => g.GroupBy(a => a.Ilce)
//             //               .ToDictionary(
//             //                   ilceGroup => ilceGroup.Key,
//             //                   ilceGroup => ilceGroup.Select(a => a.Mahalle).ToList()
//             //               )
//             //     );

//             // var model = new IlanverViewModel
//             // {
//             //     Adresler = adresler,
//             // };

//             // // Gruplandırılmış adresleri JSON olarak ViewBag'e aktar
//             // ViewBag.AdresGruplari = groupedAdresler;

//             return View();
//         }

//         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//         public IActionResult Error()
//         {
//             return View("Error!");
//         }
//     }
// }