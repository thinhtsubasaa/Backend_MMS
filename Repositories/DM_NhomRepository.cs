using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_NhomRepository : IRepository<DM_Nhom>
    {

    }
    public class DM_NhomRepository : Repository<DM_Nhom>, IDM_NhomRepository
    {
        public DM_NhomRepository(MyDbContext _db) : base(_db)
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
