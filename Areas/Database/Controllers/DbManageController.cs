using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Database.Controllers
{
    [Area("Database")]
    [Route("/Database/Database-Manage/{action=Index}")]
    public class DbManageController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManageController(AppDbContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
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

        public async Task<IActionResult> SeedData()
        {
            var roleNames = typeof(RoleName).GetFields().ToList();
            foreach (var role in roleNames)
            {
                var roleName = (string)role.GetRawConstantValue();
                var roleFound = await _roleManager.FindByNameAsync(roleName);
                if(roleFound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            //Create Admin account, Username: Admin, Role Administrator, email: ngothanhbinh261@gmail.com, Password: 123456
            var admin = await _userManager.FindByEmailAsync("ngothanhbinh261@gmail.com");
            if(admin==null)
            {
                admin = new AppUser()
                {
                    Email = "ngothanhbinh261@gmail.com",
                    UserName = "admin",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin, "123456");
                await _userManager.AddToRoleAsync(admin, RoleName.Administrator);
            }
            var editor = new AppUser()
            {
                Email = "editor@example.com",
                UserName = "editor",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(editor, "123456");
            await _userManager.AddToRoleAsync(editor, RoleName.Editor);

            var member = new AppUser()
            {
                Email = "member@example.com",
                UserName = "member",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(member, "123456");
            await _userManager.AddToRoleAsync(member, RoleName.Member);

            StatusMessage = "Seed Admin account Successfully";
            return RedirectToAction("Index"); 
        }
    }
}