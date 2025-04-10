using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IDM_HangMucRepository : IRepository<DM_HangMuc>
    {

    }
    public class DM_HangMucRepository : Repository<DM_HangMuc>, IDM_HangMucRepository
    {
        public DM_HangMucRepository(MyDbContext _db) : base(_db)
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
