using System.Collections.Generic;
using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;
using ERP.Repositories;


namespace ERP.UOW
{
    public class UnitofWork : IUnitofWork
    {
        public ILogRepository Logs { get; private set; }
        public IConfigRepository Configs { get; private set; }

        public IAdsunRepository Adsuns { get; private set; }
        public IDS_PhuongTienRepository DS_PhuongTiens { get; private set; }
        public IPhuongTien_PhuTrachRepository PhuongTien_PhuTrachs { get; private set; }
        public IPhuongTien_DonViRepository PhuongTien_DonVis { get; private set; }
        public IDS_ThietBiRepository DS_Thietbis { get; private set; }
        public IDM_DonViRepository DM_DonVis { get; private set; }
        public IDM_BoPhanRepository DM_BoPhans { get; private set; }
        public IDM_NhomRepository DM_Nhoms { get; private set; }
        public IDM_LoaiRepository DM_Loais { get; private set; }
        public IDM_ModelRepository DM_Models { get; private set; }
        public IDM_TinhTrangRepository DM_TinhTrangs { get; private set; }
        public ITranslateRepository Translates { get; private set; }
        public IStatusRepository Statuss { get; private set; }
        public IDM_BaoDuongRepository DM_BaoDuongs { get; private set; }
        public IKeHoachBaoDuongRepository KeHoachBaoDuongs { get; private set; }
        public ILichSuBaoDuongRepository LichSuBaoDuongs { get; private set; }
        public IMMS_PhuTrachBoPhanRepository MMS_PhuTrachBoPhans { get; private set; }
        public ILichSuPhanBoDonViRepository LichSuPhanBoDonVis { get; private set; }
        public IDM_TanSuatRepository DM_TanSuats { get; private set; }
        public IGhepNoiPhuongTien_ThietBiRepository GhepNoiPhuongTien_ThietBis { get; private set; }

        public IDM_HangMucRepository DM_HangMucs { get; private set; }

        public IThongTinTheoHangMucRepository ThongTinTheoHangMucs { get; private set; }
        public ILichSuBaoDuong_ChiTietRepository LichSuBaoDuong_ChiTiets { get; private set; }
        public ILichSuKiemTraHangNgayRepository LichSuKiemTraHangNgays { get; private set; }



        private MyDbContext db;

        public UnitofWork(MyDbContext _db)
        {
            db = _db;

            Logs = new LogRepository(db);
            Configs = new ConfigRepository(db);
            Adsuns = new AdsunRepository(db);
            DS_PhuongTiens = new DS_PhuongTienRepository(db);
            PhuongTien_PhuTrachs = new PhuongTien_PhuTrachRepository(db);
            PhuongTien_DonVis = new PhuongTien_DonViRepository(db);
            DS_Thietbis = new DS_ThietBiRepository(db);
            DM_DonVis = new DM_DonViRepository(db);
            DM_BoPhans = new DM_BoPhanRepository(db);
            DM_Nhoms = new DM_NhomRepository(db);
            DM_Loais = new DM_LoaiRepository(db);
            DM_Models = new DM_ModelRepository(db);
            DM_TinhTrangs = new DM_TinhTrangRepository(db);
            Translates = new TranslateRepository(db);
            Statuss = new StatusRepository(db);
            DM_BaoDuongs = new DM_BaoDuongRepository(db);
            KeHoachBaoDuongs = new KeHoachBaoDuongRepository(db);
            LichSuBaoDuongs = new LichSuBaoDuongRepository(db);
            MMS_PhuTrachBoPhans = new MMS_PhuTrachBoPhanRepository(db);
            LichSuPhanBoDonVis = new LichSuPhanBoDonViRepository(db);
            DM_TanSuats = new DM_TanSuatRepository(db);
            GhepNoiPhuongTien_ThietBis = new GhepNoiPhuongTien_ThietBiRepository(db);
            DM_HangMucs = new DM_HangMucRepository(db);
            ThongTinTheoHangMucs = new ThongTinTheoHangMucRepository(db);
            LichSuBaoDuong_ChiTiets = new LichSuBaoDuong_ChiTietRepository(db);
            LichSuKiemTraHangNgays = new LichSuKiemTraHangNgayRepository(db);

        }
        public void Dispose()
        {
            db.Dispose();
        }
        public int Complete()
        {
            return db.SaveChanges();
        }
    }
}