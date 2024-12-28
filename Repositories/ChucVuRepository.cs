using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IChucVuRepository : IRepository<ChucVu>
    {

    }
    public class ChucVuRepository : Repository<ChucVu>, IChucVuRepository
    {
        public ChucVuRepository(MyDbContext _db) : base(_db)
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
