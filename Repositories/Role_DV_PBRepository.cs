using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{

    public interface IRole_DV_PBRepository : IRepository<Role_DV_PB>
    {

    }
    public class Role_DV_PBRepository : Repository<Role_DV_PB>, IRole_DV_PBRepository
    {
        public Role_DV_PBRepository(MyDbContext _db) : base(_db)
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
