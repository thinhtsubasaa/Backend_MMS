using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IConfigRepository : IRepository<Config>
    {

    }
    public class ConfigRepository : Repository<Config>, IConfigRepository
    {
        public ConfigRepository(MyDbContext _db) : base(_db)
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
