using BookStore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repository.IRepositpry
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company company);
    }
}
