using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Areas.Blog.Models;
using App.Data;
using App.Models;
using App.Models.Blog;
using Bogus;
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
                if (roleFound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            //Create Admin account, Username: Admin, Role Administrator, email: ngothanhbinh261@gmail.com, Password: 123456
            var admin = await _userManager.FindByEmailAsync("ngothanhbinh261@gmail.com");
            if (admin == null)
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
            // var editor = new AppUser()
            // {
            //     Email = "editor@example.com",
            //     UserName = "editor",
            //     EmailConfirmed = true
            // };
            // await _userManager.CreateAsync(editor, "123456");
            // await _userManager.AddToRoleAsync(editor, RoleName.Editor);

            // var member = new AppUser()
            // {
            //     Email = "member@example.com",
            //     UserName = "member",
            //     EmailConfirmed = true
            // };
            // await _userManager.CreateAsync(member, "123456");
            // await _userManager.AddToRoleAsync(member, RoleName.Member);
            SeedPostAndCategory();

            StatusMessage = "Seed Admin account Successfully";
            return RedirectToAction("Index");
        }

        private void SeedPostAndCategory()
        {
            //Seed Category
            _dbContext.RemoveRange(_dbContext.Categories.Where(c => c.Discription.Contains("fakeData")));
            _dbContext.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("fakeData")));
            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM {cm++}" + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Discription, fk => fk.Lorem.Sentences(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());



            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();
            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;
            var categories = new Category[] { cate1, cate2, cate12, cate11, cate21, cate211 };
            _dbContext.Categories.AddRange(categories);

            //Seed Post and Realation Beetween Post and Cate
            var rCateIndex = new Random();
            int bv = 1;
            var user = _userManager.GetUserAsync(this.User).Result;
            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + " [fakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2021, 7, 1))); fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));
            List<Post> posts = new List<Post>();
            List<PostCategory> post_categories = new List<PostCategory>();



            for (int i = 0; i < 40; i++)
            {
                var post =  fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                post_categories.Add(new PostCategory()
                {
                    Post = post,
                    Category = categories[rCateIndex.Next(5)]
                });
                
            }
            _dbContext.AddRange(posts);
            _dbContext.AddRange(post_categories);
            // END POST


            
            _dbContext.SaveChanges();

        }
    }
}