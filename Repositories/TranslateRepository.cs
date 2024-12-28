using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ITranslateRepository : IRepository<Translate>
    {

    }
    public class TranslateRepository : Repository<Translate>, ITranslateRepository
    {
        public TranslateRepository(MyDbContext _db) : base(_db)
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
