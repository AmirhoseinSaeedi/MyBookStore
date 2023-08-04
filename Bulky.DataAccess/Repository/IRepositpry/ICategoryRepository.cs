using BookStore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repository.IRepositpry
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
    }
}
