using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Areas.Blog.Models;
using App.Data;
using Microsoft.AspNetCore.Authorization;
using App.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;

namespace App.Areas.Blog.Controllers
{   
    [Area("Blog")]
    [Route("/Admin/Blog/Post/{action=Index}/{id?}")]
    [Authorize(Roles = RoleName.Administrator+ ","+RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage {get; set;}

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")]int currentPage, int pageSize)
        {
            var posts = _context.Posts.Include(p => p.Author)
                                      .OrderByDescending(p => p.DateUpdated);
            int totalPosts = posts.Count();
            if(pageSize <= 0) pageSize = 10;
            
            int countPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new {
                    p = pageNumber,
                    pageSize = pageSize
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            var postsInPage = await posts.Skip((currentPage - 1) * pageSize)
                        .Take(pageSize)
                        .Include(p => p.PostCategories)
                        .ThenInclude(pc => pc.Category)
                        .ToListAsync();
            // model.totalUsers = await qr.CountAsync();
            // model.countPages = (int)Math.Ceiling((double)model.totalUsers / model.ITEMS_PER_PAGE);

            // if (model.currentPage < 1)
            //     model.currentPage = 1;
            // if (model.currentPage > model.countPages)
            //     model.currentPage = model.countPages;

            // var qr1 = qr.Skip((model.currentPage - 1) * model.ITEMS_PER_PAGE)
            //             .Take(model.ITEMS_PER_PAGE)
            //             .Select(u => new UserAndRole() {
            //                 Id = u.Id,
            //                 UserName = u.UserName,
            //             });
            return View(postsInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["listCategory"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,listCategoryId")] CreatePostModel post)
        {
             var categories = await _context.Categories.ToListAsync();
            ViewData["listCategory"] = new MultiSelectList(categories, "Id", "Title");
            if(post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if(await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "URL bị trùng, nhập URL khác!!!");
                return View(post);
            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Posts.Add(post);
                // await _context.SaveChangesAsync();
                
                if(post.listCategoryId != null)
                {
                    foreach (var id in post.listCategoryId)
                    {
                        var postCate = new PostCategory()
                        {
                            Post = post,
                            CategoryID = id
                        };
                        System.Console.WriteLine("*************************************");
                         System.Console.WriteLine("new postID =",postCate.PostID);
                        _context.PostCategories.Add(postCate);
                    }
                   
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Vừa thêm bài viết mới";
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            // var post = await _context.Posts.FindAsync(id);
            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            var postEditModel = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                listCategoryId = post.PostCategories.Select(pc => pc.CategoryID).ToList()
            };
             var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View(postEditModel);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published, listCategoryId")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            var categories = await _context.Categories.ToListAsync();
            ViewData["listCategoryId"] = new MultiSelectList(categories, "Id", "Title");

            if(post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if(await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "URL bị trùng, nhập URL khác!!!");
                return View(post);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if(postUpdate == null)
                    {
                        return NotFound();
                    }
                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.DateUpdated = DateTime.Now;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;

                    //Update Post Category
                    if(post.listCategoryId == null) post.listCategoryId = new List<int>();
                    var oldCates =  postUpdate.PostCategories.Select(c => c.CategoryID).ToList();
                    var newCates = post.listCategoryId;

                    //Cac cate can loai bo
                    var removedCates = from c in postUpdate.PostCategories
                                      where (!newCates.Contains(c.CategoryID))
                                      select c;
                    _context.PostCategories.RemoveRange(removedCates);
                    //Cac cate can them vao
                    var addedCates = from c in newCates
                                   where (!oldCates.Contains(c))
                                   select c;
                    foreach (var cateId in addedCates)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            PostID = id,
                            CategoryID = cateId
                        });
                    }

                    // _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật bài viết";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }
            StatusMessage = $"Post {post.Title} Deleted!";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
