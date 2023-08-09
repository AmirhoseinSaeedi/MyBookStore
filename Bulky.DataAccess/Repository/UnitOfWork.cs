using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repository.IRepositpry;

namespace BookStore.Infrastructure.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }

        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; private set; }
        public IshoppingCartRepository ShoppingCart { get;private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }

        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category = new CategoryRepository(_context);
            Product = new ProductRepository(_context);
            Company = new CompanyRepository(_context);
            ShoppingCart = new ShoppingCartRepository(_context);
            ApplicationUser = new ApplicationUserRepository(_context);
        }


        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
