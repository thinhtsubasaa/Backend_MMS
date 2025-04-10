using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ILichSuBaoDuong_ChiTietRepository : IRepository<LichSuBaoDuong_ChiTiet>
    {

    }
    public class LichSuBaoDuong_ChiTietRepository : Repository<LichSuBaoDuong_ChiTiet>, ILichSuBaoDuong_ChiTietRepository
    {
        public LichSuBaoDuong_ChiTietRepository(MyDbContext _db) : base(_db)
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
