using BookStore.Domain;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repository.IRepositpry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productFromDb = _context.Products.FirstOrDefault(u=>u.Id== product.Id);
            if (productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.ISBN = product.ISBN;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Description = product.Description;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.Author = product.Author;
                if(product.ImageUrl != null)
                {
                    productFromDb.ImageUrl = product.ImageUrl;
                }
            }
        }

        
    }
}
