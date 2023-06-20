using System.ComponentModel.DataAnnotations;
using App.Models.Blog;

namespace App.Areas.Blog.Models
{
    public class CreatePostModel : Post
    {
        [Display(Name = "Chuyên mục")]
        public List<int> listCategoryId { get; set; }
        // public Category Categories { get; set; }
    }
}