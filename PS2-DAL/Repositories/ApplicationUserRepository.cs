using PS2_DAL.Data;
using PS2_DAL.Models;
using PS2_DAL.Repositories.IRepository;

namespace PS2_DAL.Repositories
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {

        private readonly ApplicationDbContext _db;

        public ApplicationUserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;

        }
    }
}
