using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IKeHoachBaoDuongRepository : IRepository<KeHoachBaoDuong>
    {

    }
    public class KeHoachBaoDuongRepository : Repository<KeHoachBaoDuong>, IKeHoachBaoDuongRepository
    {
        public KeHoachBaoDuongRepository(MyDbContext _db) : base(_db)
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
