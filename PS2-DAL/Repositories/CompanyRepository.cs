using PS2_DAL.Data;
using PS2_DAL.Models;
using PS2_DAL.Repositories.IRepository;

namespace PS2_DAL.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {

        private readonly ApplicationDbContext _db;

        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;

        }

        public void Update(Company obj)
        {
            _db.Companies.Update(obj);
        }
    }
}
