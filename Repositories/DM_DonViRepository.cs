using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_DonViRepository : IRepository<DM_DonVi>
    {

    }
    public class DM_DonViRepository : Repository<DM_DonVi>, IDM_DonViRepository
    {
        public DM_DonViRepository(MyDbContext _db) : base(_db)
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
