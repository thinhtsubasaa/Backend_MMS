using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IGhepNoiPhuongTien_ThietBiRepository : IRepository<GhepNoiPhuongTien_ThietBi>
    {

    }
    public class GhepNoiPhuongTien_ThietBiRepository : Repository<GhepNoiPhuongTien_ThietBi>, IGhepNoiPhuongTien_ThietBiRepository
    {
        public GhepNoiPhuongTien_ThietBiRepository(MyDbContext _db) : base(_db)
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
