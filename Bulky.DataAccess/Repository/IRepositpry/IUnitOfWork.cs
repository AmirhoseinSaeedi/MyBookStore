namespace BookStore.Infrastructure.Repository.IRepositpry
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        void Save();
    }
}
