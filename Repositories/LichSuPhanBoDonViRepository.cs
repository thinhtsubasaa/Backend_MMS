using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ILichSuPhanBoDonViRepository : IRepository<LichSuPhanBoDonVi>
    {

    }
    public class LichSuPhanBoDonViRepository : Repository<LichSuPhanBoDonVi>, ILichSuPhanBoDonViRepository
    {
        public LichSuPhanBoDonViRepository(MyDbContext _db) : base(_db)
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
