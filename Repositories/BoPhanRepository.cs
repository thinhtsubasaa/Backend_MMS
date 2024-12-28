using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IBoPhanRepository : IRepository<BoPhan>
    {

    }
    public class BoPhanRepository : Repository<BoPhan>, IBoPhanRepository
    {
        public BoPhanRepository(MyDbContext _db) : base(_db)
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