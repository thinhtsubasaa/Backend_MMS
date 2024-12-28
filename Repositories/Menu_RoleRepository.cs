using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface IMenu_RoleRepository : IRepository<Menu_Role>
    {

    }
    public class Menu_RoleRepository : Repository<Menu_Role>, IMenu_RoleRepository
    {
        public Menu_RoleRepository(MyDbContext _db) : base(_db)
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