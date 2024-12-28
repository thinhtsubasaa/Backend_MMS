using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IChiTiet_DV_PB_BPRepository : IRepository<ChiTiet_DV_PB_BP>
    {

    }
    public class ChiTiet_DV_PB_BPRepository : Repository<ChiTiet_DV_PB_BP>, IChiTiet_DV_PB_BPRepository
    {
        public ChiTiet_DV_PB_BPRepository(MyDbContext _db) : base(_db)
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
