using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DBGoreWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DBGoreWebApp.Models.Entities;

namespace DBGoreWebApp.Controllers
{
    public class KategoriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KategoriController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult KategoriSec(string kategoriad, string altkategori)
        {
            if (string.IsNullOrEmpty(kategoriad) || string.IsNullOrEmpty(altkategori))
            {
                return RedirectToAction("Index", "Home"); // Kategori veya alt kategori eksikse ana sayfaya yönlendir
            }

            // Kategori tablosundan ilgili alt kategorinin Controller ve Action bilgilerini al
            var kategori = _context.Kategoriler
                .FirstOrDefault(k => k.KategoriAd == kategoriad && k.AltKategori == altkategori);

            if (kategori == null)
            {
                return RedirectToAction("NotFound", "Home"); // Kategori bulunamazsa hata sayfasına yönlendir
            }

            return RedirectToAction(kategori.Action, kategori.Controller, new { altkategori });
        }
        public IActionResult Kategoriler()
        {
            var kategoriler = _context.Kategoriler.ToList();

            // KategoriAd'a göre gruplandırma
            var groupedKategoriler = kategoriler
                .GroupBy(k => k.KategoriAd)
                .ToList();

            return View(groupedKategoriler);
        }

        // Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id,KategoriAd,AltKategori,Controller,Action")] Kategori kategori)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kategori);
                await _context.SaveChangesAsync();
                return RedirectToAction("KategoriAyarlari", "Admin");
            }
            return PartialView("Admin/Ayarlar/_Kategoriler", await _context.Kategoriler.ToListAsync());
        }

        // Update
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,KategoriAd,AltKategori,Controller,Action")] Kategori kategori)
        {
            if (id != kategori.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kategori);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KategoriExists(kategori.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("KategoriAyarlari", "Admin");
            }
            return PartialView("Admin/Ayarlar/_Kategoriler", await _context.Kategoriler.ToListAsync());
        }

        // Delete
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kategori = await _context.Kategoriler.FindAsync(id);
            _context.Kategoriler.Remove(kategori);
            await _context.SaveChangesAsync();
            return RedirectToAction("KategoriAyarlari", "Admin");
        }

        private bool KategoriExists(int id)
        {
            return _context.Kategoriler.Any(e => e.Id == id);
        }
    }
}
