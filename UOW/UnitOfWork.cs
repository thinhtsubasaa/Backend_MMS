using System.Collections.Generic;
using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;
using ERP.Repositories;


namespace ERP.UOW
{
    public class UnitofWork : IUnitofWork
    {
        public IMenuRepository Menus { get; private set; }
        public IDonViRepository DonVis { get; private set; }

        public IMenu_RoleRepository Menu_Roles { get; private set; }
        public ILogRepository Logs { get; private set; }
        public IBoPhanRepository BoPhans { get; private set; }
        public ITapDoanRepository tapDoans { get; private set; }
        public IPhongbanRepository phongbans { get; private set; }
        public IChucVuRepository chucVus { get; private set; }

        public ICBNV_DieuChuyenRepository cBNV_DieuChuyens { get; private set; }
        public IConfigRepository Configs { get; private set; }
        public IRoleByDonViRepository roleByDonVis { get; private set; }
        public IRole_DV_PBRepository role_DV_PBs { get; private set; }
        public IChiTiet_DV_PB_BPRepository chiTiet_DV_PB_BPs { get; private set; }
        public INhomDoiTacRepository NhomDoiTacs { get; private set; }
        public ITaiXeRepository TaiXes { get; private set; }
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



        private MyDbContext db;

        public UnitofWork(MyDbContext _db)
        {
            db = _db;
            Menus = new MenuRepository(db);

            DonVis = new DonViRepository(db);
            Menu_Roles = new Menu_RoleRepository(db);
            Logs = new LogRepository(db);
            BoPhans = new BoPhanRepository(db);
            tapDoans = new TapDoanRepository(db);
            phongbans = new PhongbanRepository(db);
            chucVus = new ChucVuRepository(db);

            cBNV_DieuChuyens = new CBNV_DieuChuyenRepository(db);
            Configs = new ConfigRepository(db);
            roleByDonVis = new RoleByDonViRepository(db);
            role_DV_PBs = new Role_DV_PBRepository(db);
            chiTiet_DV_PB_BPs = new ChiTiet_DV_PB_BPRepository(db);
            NhomDoiTacs = new NhomDoiTacRepository(db);
            TaiXes = new TaiXeRepository(db);
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