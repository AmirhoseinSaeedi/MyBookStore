using BookStore.Domain;
using BookStore.Infrastructure.Data;
using BookStore.Infrastructure.Repository.IRepositpry;

namespace BookStore.Infrastructure.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IshoppingCartRepository
    {
        private readonly ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(ShoppingCart shoppingCart)
        {
            _context.ShoppingCarts.Update(shoppingCart);
        }
    }
}
