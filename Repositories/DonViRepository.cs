using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDonViRepository : IRepository<DonVi>
    {

    }
    public class DonViRepository : Repository<DonVi>, IDonViRepository
    {
        public DonViRepository(MyDbContext _db) : base(_db)
        {
        }
        public MyDbContext MyDbContext
        {
            get
            {
                return _db as MyDbContext;
            }
        }


    }
}