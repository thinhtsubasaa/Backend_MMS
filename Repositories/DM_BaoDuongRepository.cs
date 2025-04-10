using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_BaoDuongRepository : IRepository<DM_BaoDuong>
    {

    }
    public class DM_BaoDuongRepository : Repository<DM_BaoDuong>, IDM_BaoDuongRepository
    {
        public DM_BaoDuongRepository(MyDbContext _db) : base(_db)
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
