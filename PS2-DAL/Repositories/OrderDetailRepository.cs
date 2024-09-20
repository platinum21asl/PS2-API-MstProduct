using PS2_DAL.Data;
using PS2_DAL.Models;
using PS2_DAL.Repositories.IRepository;

namespace PS2_DAL.Repositories
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {

        private readonly ApplicationDbContext _db;

        public OrderDetailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;

        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
