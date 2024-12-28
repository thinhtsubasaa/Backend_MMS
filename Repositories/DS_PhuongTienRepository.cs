using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDS_PhuongTienRepository : IRepository<DS_PhuongTien>
    {

    }
    public class DS_PhuongTienRepository : Repository<DS_PhuongTien>, IDS_PhuongTienRepository
    {
        public DS_PhuongTienRepository(MyDbContext _db) : base(_db)
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
