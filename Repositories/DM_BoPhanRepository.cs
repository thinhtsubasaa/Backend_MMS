using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_BoPhanRepository : IRepository<DM_BoPhan>
    {

    }
    public class DM_BoPhanRepository : Repository<DM_BoPhan>, IDM_BoPhanRepository
    {
        public DM_BoPhanRepository(MyDbContext _db) : base(_db)
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
