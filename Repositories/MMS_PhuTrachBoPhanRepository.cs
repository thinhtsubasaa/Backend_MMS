using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IMMS_PhuTrachBoPhanRepository : IRepository<MMS_PhuTrachBoPhan>
    {

    }
    public class MMS_PhuTrachBoPhanRepository : Repository<MMS_PhuTrachBoPhan>, IMMS_PhuTrachBoPhanRepository
    {
        public MMS_PhuTrachBoPhanRepository(MyDbContext _db) : base(_db)
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