using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ICBNV_DieuChuyenRepository : IRepository<CBNV_DieuChuyen>
    {

    }
    public class CBNV_DieuChuyenRepository : Repository<CBNV_DieuChuyen>, ICBNV_DieuChuyenRepository
    {
        public CBNV_DieuChuyenRepository(MyDbContext _db) : base(_db)
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
