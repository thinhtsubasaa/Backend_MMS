using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IPhuongTien_DonViRepository : IRepository<PhuongTien_DonVi>
    {

    }
    public class PhuongTien_DonViRepository : Repository<PhuongTien_DonVi>, IPhuongTien_DonViRepository
    {
        public PhuongTien_DonViRepository(MyDbContext _db) : base(_db)
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
