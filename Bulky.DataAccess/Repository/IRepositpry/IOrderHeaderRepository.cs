using BookStore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Infrastructure.Repository.IRepositpry
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

	}
}
