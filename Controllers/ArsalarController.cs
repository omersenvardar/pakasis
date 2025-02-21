using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using Microsoft.EntityFrameworkCore;
using DBGoreWebApp.Models;
using DBGoreWebApp.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DBGoreWebApp.Models.ViewModels;

namespace DBGoreWebApp.Controllers
{
    // [Route("[controller]")]
    public class ArsalarController : Controller
    {
        private readonly ILogger<ArsalarController> _logger;
        private readonly ApplicationDbContext _context;

        public ArsalarController(ApplicationDbContext context, ILogger<ArsalarController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var adresler = _context.Adresler
    .GroupBy(a => a.Il) // İl bazında grupla
    .Select(g => g.FirstOrDefault())
    .ToList();
            // var adresler = _context.Adresler.ToList();
            return View(adresler);
        }
        
        [HttpPost]
        public IActionResult GetIlce(Adres model)
        {
            var ilceler = _context.Adresler
                .Where(a => a.Il == model.Il)
                .GroupBy(a => a.Ilce) // İlçe bazında grupla
                .Select(g => g.FirstOrDefault())
                .ToList();

            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}