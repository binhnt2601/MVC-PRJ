using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controlers
{
    public class PlanetController : Controller
    {
        private readonly ILogger<PlanetController> _logger;
        private readonly PlanetService _service;

        public PlanetController(ILogger<PlanetController> logger, PlanetService service)
        {
            _logger = logger;
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            var planet = _service.Where(p => p.Id == id).FirstOrDefault();
            
            return View(planet);
        }
    }
}