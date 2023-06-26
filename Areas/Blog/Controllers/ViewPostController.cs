using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Areas.Blog.Models;
using App.Data;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BasicMVC.Areas_Blog_Controllers
{
    [Area("Blog")]
    [Route("/posts")]
    public class ViewPostController : Controller
    {

        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;
        private IMemoryCache _cache;


        // Số bài hiện thị viết trên một trang danh mục
        public const int ITEMS_PER_PAGE = 4;

        public ViewPostController(ILogger<ViewPostController> logger,
            AppDbContext context,
            IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        [Route("{slug?}", Name = "listpost")]
        public IActionResult Index([FromRoute(Name = "slug")] string slugCategory, [FromQuery(Name = "p")]int currentPage, int pageSize)
        {

            var categories = GetCategories();

            Category category = null;
            if (!string.IsNullOrEmpty(slugCategory))
            {

                category = FindCategoryBySlug(categories, slugCategory);
                if (category == null)
                {
                    return NotFound("Không thấy Category");
                }

            }
            ViewData["categories"] = categories;
            ViewData["slugCategory"] = slugCategory;
            ViewData["CurrentCategory"] = category;


            // ........................................
            // Truy vấn lấy các post
            var posts = _context.Posts
                .Include(p => p.Author) // Load Author cho post  
                .Include(p => p.PostCategories) // Load các Category của Post
                .ThenInclude(c => c.Category)
                .AsQueryable();

            if (category != null)
            {

                var ids = category.ChildCategoryIDs();
                ids.Add(category.Id);

                // Lọc các Post có trong category (và con của nó)
                posts = posts.Where(p => p.PostCategories
                    .Where(c => ids.Contains(c.CategoryID)).Any());

            }

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

            var postsInPage = posts.Skip((currentPage - 1) * pageSize)
                        .Take(pageSize);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            

            // Thực hiện truy vấn lấy List các Post và chuyển cho View
            return View(postsInPage.ToList());
        }

        [Route("post/{postSlug}")]
        public IActionResult Details(string postSlug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var post = _context.Posts.Where(p => p.Slug == postSlug)
                       .Include(p => p.Author)
                       .Include(p => p.PostCategories)
                       .ThenInclude(pc => pc.Category)
                       .FirstOrDefault();
            if (post == null)
            {
                return NotFound();
            }
            Category category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var relativePosts = _context.Posts.Where(p => p.PostCategories.Any(pc => pc.CategoryID == category.Id))
                                              .Where(p => p.PostId != post.PostId)
                                              .OrderBy(p => p.DateUpdated)
                                              .Take(5)
                                              .ToList();
            ViewBag.relativePosts = relativePosts;

            return View(post);
        }

        [NonAction]
        List<Category> GetCategories()
        {

            List<Category> categories;

            string keycacheCategories = "_listallcategories";

            // Phục hồi categories từ Memory cache, không có thì truy vấn Db
            if (!_cache.TryGetValue(keycacheCategories, out categories))
            {

                categories = _context.Categories
                    .Include(c => c.CategoryChildren)
                    .AsEnumerable()
                    .Where(c => c.ParentCategory == null)
                    .ToList();

                // Thiết lập cache - lưu vào cache
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(300));
                _cache.Set("_GetCategories", categories, cacheEntryOptions);
            }

            return categories;
        }


        // Tìm (đệ quy) trong cây, một Category theo Slug
        [NonAction]
        Category FindCategoryBySlug(List<Category> categories, string Slug)
        {

            foreach (var c in categories)
            {
                if (c.Slug == Slug) return c;
                var c1 = FindCategoryBySlug(c.CategoryChildren.ToList(), Slug);
                if (c1 != null)
                    return c1;
            }

            return null;
        }
    }
}
