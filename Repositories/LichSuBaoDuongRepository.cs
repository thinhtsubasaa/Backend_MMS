using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ILichSuBaoDuongRepository : IRepository<LichSuBaoDuong>
    {

    }
    public class LichSuBaoDuongRepository : Repository<LichSuBaoDuong>, ILichSuBaoDuongRepository
    {
        public LichSuBaoDuongRepository(MyDbContext _db) : base(_db)
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
