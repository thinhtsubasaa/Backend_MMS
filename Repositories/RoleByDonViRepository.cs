using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IRoleByDonViRepository : IRepository<RoleByDonVi>
    {

    }
    public class RoleByDonViRepository : Repository<RoleByDonVi>, IRoleByDonViRepository
    {
        public RoleByDonViRepository(MyDbContext _db) : base(_db)
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
