using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IStatusRepository : IRepository<Status>
    {

    }
    public class StatusRepository : Repository<Status>, IStatusRepository
    {
        public StatusRepository(MyDbContext _db) : base(_db)
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
