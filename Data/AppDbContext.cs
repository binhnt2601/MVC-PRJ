using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using App.Models.Blog;
using App.Models;
using App.Models.Contacts;
using App.Areas.Blog.Models;

namespace App.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            // Bỏ tiền tố AspNet của các bảng: mặc định
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            builder.Entity<Category>(entity => {
                entity.HasIndex(c => c.Slug)
                      .IsUnique();
            });

            builder.Entity<PostCategory>(entity => {
                // entity.HasKey(pc => pc.PostID);
                // entity.HasKey(pc => pc.CategoryID);

                entity.HasKey(pc => new {pc.PostID, pc.CategoryID});
            });

            builder.Entity<Post>(entity => {
                entity.HasIndex(p => p.Slug)
                      .IsUnique();
            });
        }

        public DbSet<Contact> Contacts {get; set;}
        public DbSet<Category> Categories {get; set;}
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories {get; set;}

    }

}