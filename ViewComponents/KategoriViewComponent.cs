using DBGoreWebApp.Data;
using DBGoreWebApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DBGoreWebApp.ViewComponents
{
    public class KategoriViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public KategoriViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync() 
    { 
        // Kategorileri ve alt kategorileri gruplayarak alıyoruz
        var kategoriler = await _context.Kategoriler
            .GroupBy(k => k.KategoriAd)
            .ToListAsync();

        return View(kategoriler); 
    }
}

}
