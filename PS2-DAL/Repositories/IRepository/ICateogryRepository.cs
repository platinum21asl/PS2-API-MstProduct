using PS2_DAL.Models;

namespace PS2_DAL.Repositories.IRepository
{
    public interface ICateogryRepository : IRepository<Category>
    {
        void Update(Category obj);
    }
}
