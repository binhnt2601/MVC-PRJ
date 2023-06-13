using App.Models;

namespace App.Services
{
    public class ProductService : List<Product>
    {
        public ProductService()
        {
                this.AddRange(new Product[] {
                    new Product(){Id = 1, Name = "Xiaomi", Info = "Phone"},
                    new Product(){Id = 2, Name = "Sanyo", Info = "Washing Machine"},
                    new Product(){Id = 3, Name = "Apple", Info = "Monitor"},
                    new Product(){Id = 4, Name = "Dell", Info = "Laptop"},
                    new Product(){Id = 5, Name = "Dyson", Info = "Hair Dryer"},
                    new Product(){Id = 6, Name = "Mouse", Info = "Alienware"}
                });
        }
    }
}