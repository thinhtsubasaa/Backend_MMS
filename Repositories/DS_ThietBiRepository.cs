using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDS_ThietBiRepository : IRepository<DS_ThietBi>
    {

    }
    public class DS_ThietBiRepository : Repository<DS_ThietBi>, IDS_ThietBiRepository
    {
        public DS_ThietBiRepository(MyDbContext _db) : base(_db)
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
