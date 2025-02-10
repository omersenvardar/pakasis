using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DBGoreWebApp.Controllers
{
    
    public class DuyurularController : Controller
    {
        private readonly ILogger<DuyurularController> _logger;
        private readonly DuyurularController _context;

        public DuyurularController(ILogger<DuyurularController> logger, DuyurularController context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}