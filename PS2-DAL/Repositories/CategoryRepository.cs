using PS2_DAL.Data;
using PS2_DAL.Models;
using PS2_DAL.Repositories.IRepository;

namespace PS2_DAL.Repositories
{
    public class CategoryRepository : Repository<Category>, ICateogryRepository
    {

        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;

        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
