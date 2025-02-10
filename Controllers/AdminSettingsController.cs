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
    // Admin sayfa ayarları için kullanılan controller
public class AdminSettingsController : Controller
{
    private readonly ILogger<AdminSettingsController> _logger;
        private readonly ApplicationDbContext _context;

        public AdminSettingsController(ApplicationDbContext context, ILogger<AdminSettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

    [HttpPost]
    public IActionResult UpdateSetting(string key, string value)
    {
        var setting = _context.AdminSettings.FirstOrDefault(s => s.SettingKey == key);
        if (setting != null)
        {
            setting.SettingValue = value;
            _context.SaveChanges();
        }
        else
        {
            _context.AdminSettings.Add(new AdminSettings { SettingKey = key, SettingValue = value });
            _context.SaveChanges();
        }
        return Ok();
    }


    [HttpGet]
    public IActionResult GetSetting(string key)
    {
        var setting = _context.AdminSettings.FirstOrDefault(s => s.SettingKey == key);
        if (setting != null)
        {
            return Ok(setting.SettingValue);
        }
        return NotFound();
    }
}
}