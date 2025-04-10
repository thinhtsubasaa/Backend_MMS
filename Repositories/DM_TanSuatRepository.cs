using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_TanSuatRepository : IRepository<DM_TanSuat>
    {

    }
    public class DM_TanSuatRepository : Repository<DM_TanSuat>, IDM_TanSuatRepository
    {
        public DM_TanSuatRepository(MyDbContext _db) : base(_db)
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
