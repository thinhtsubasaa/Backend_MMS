using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_ModelRepository : IRepository<DM_Model>
    {

    }
    public class DM_ModelRepository : Repository<DM_Model>, IDM_ModelRepository
    {
        public DM_ModelRepository(MyDbContext _db) : base(_db)
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
