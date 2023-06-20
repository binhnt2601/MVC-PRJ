using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("/admin/blog/category/{action=index}/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.Categories
                      select c).Include(c => c.ParentCategory)
                     .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                             .Where(c => c.ParentCategory == null)
                             .ToList();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public async Task<IActionResult> Create()
        {
            var qr = (from c in _context.Categories
                      select c).Include(c => c.ParentCategory)
                     .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                             .Where(c => c.ParentCategory == null)
                             .ToList();
            var items = new List<Category>();
            ShowCategoryWithChildren(categories, items, 0);

            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            return View();
        }

        private void ShowCategoryWithChildren(List<Category> source, List<Category> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("---", level));
            foreach (var category in source)
            {
                // category.Title = prefix + " " + category.Title;
                des.Add(new Category(){
                    Id = category.Id,
                    Title = prefix + " " + category.Title
                });
                if (category.CategoryChildren?.Count > 0)
                {
                    ShowCategoryWithChildren(category.CategoryChildren.ToList(), des, level + 1);
                }
            }
        }
        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Discription,Slug,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                     .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                             .Where(c => c.ParentCategory == null)
                             .ToList();

            var selectList = new SelectList(categories, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            // ViewData["ParentCategoryTitle"] = new SelectListItem(_context.Categories)
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.Categories
                      select c).Include(c => c.ParentCategory)
                     .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                             .Where(c => c.ParentCategory == null)
                             .ToList();
            var items = new List<Category>();
            ShowCategoryWithChildren(categories, items, 0);

            var selectList = new SelectList(items, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Discription,Slug,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)
                     .Include(c => c.CategoryChildren);
            var categories = (await qr.ToListAsync())
                             .Where(c => c.ParentCategory == null)
                             .ToList();

            var items = new List<Category>();
            ShowCategoryWithChildren(categories, items, 0);
            var selectList = new SelectList(categories, "Id", "Title");
            ViewData["ParentCategoryId"] = selectList;
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category = await _context.Categories
                                .Include(c => c.CategoryChildren)
                                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            foreach (var childCategory in category.CategoryChildren)
            {
                childCategory.ParentCategoryId = category.ParentCategoryId;
            }
            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
