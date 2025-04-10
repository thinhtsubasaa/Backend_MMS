using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ILichSuKiemTraHangNgayRepository : IRepository<LichSuKiemTraHangNgay>
    {

    }
    public class LichSuKiemTraHangNgayRepository : Repository<LichSuKiemTraHangNgay>, ILichSuKiemTraHangNgayRepository
    {
        public LichSuKiemTraHangNgayRepository(MyDbContext _db) : base(_db)
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
