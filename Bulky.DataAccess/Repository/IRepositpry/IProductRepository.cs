using BookStore.Domain;

namespace BookStore.Infrastructure.Repository.IRepositpry
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
    }
}
