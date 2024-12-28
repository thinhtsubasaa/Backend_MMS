using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_TinhTrangRepository : IRepository<DM_TinhTrang>
    {

    }
    public class DM_TinhTrangRepository : Repository<DM_TinhTrang>, IDM_TinhTrangRepository
    {
        public DM_TinhTrangRepository(MyDbContext _db) : base(_db)
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
