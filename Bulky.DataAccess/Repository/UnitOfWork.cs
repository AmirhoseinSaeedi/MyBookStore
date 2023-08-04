using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repository.IRepositpry;

namespace BookStore.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }

        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Product = new ProductRepository(_context);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
