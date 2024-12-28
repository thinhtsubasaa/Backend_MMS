using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_LoaiRepository : IRepository<DM_Loai>
    {

    }
    public class DM_LoaiRepository : Repository<DM_Loai>, IDM_LoaiRepository
    {
        public DM_LoaiRepository(MyDbContext _db) : base(_db)
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
