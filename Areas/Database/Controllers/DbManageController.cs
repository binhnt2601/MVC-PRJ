using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Database.Controllers
{
    [Area("Database")]
    [Route("/Database/Database-Manage/{action=Index}")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;

        public DbManageController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDatabase()
        {
            return View();
        }
        [TempData]
        public string StatusMessage { get; set; }

        [HttpPost]
        public async Task<IActionResult> DeleteDatabaseAsync()
        {
            var result = await _dbContext.Database.EnsureDeletedAsync();
            StatusMessage = result ? "Database Deleted" : "Cant Delete Database";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateDatabase()
        {
            await _dbContext.Database.MigrateAsync();
            StatusMessage = "Database Created Successfully";
            return RedirectToAction("Index");
        }
    }
}