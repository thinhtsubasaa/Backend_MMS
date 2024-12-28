using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
  public interface ILogRepository : IRepository<Log>
    {

    }
    public class LogRepository : Repository<Log>, ILogRepository
    {
        public LogRepository(MyDbContext _db) : base(_db)
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