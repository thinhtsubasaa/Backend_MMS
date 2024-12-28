using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IPhuongTien_PhuTrachRepository : IRepository<PhuongTien_PhuTrach>
    {

    }
    public class PhuongTien_PhuTrachRepository : Repository<PhuongTien_PhuTrach>, IPhuongTien_PhuTrachRepository
    {
        public PhuongTien_PhuTrachRepository(MyDbContext _db) : base(_db)
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
