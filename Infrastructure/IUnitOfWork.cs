using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.Repositories;

namespace ERP.Infrastructure
{
  public interface IUnitofWork : IDisposable
  {
    IMenuRepository Menus { get; }

    IPhongbanRepository phongbans { get; }
    IDonViRepository DonVis { get; }
    IMenu_RoleRepository Menu_Roles { get; }
    ILogRepository Logs { get; }
    IBoPhanRepository BoPhans { get; }
    ITapDoanRepository tapDoans { get; }
    IChucVuRepository chucVus { get; }

    ICBNV_DieuChuyenRepository cBNV_DieuChuyens { get; }
    IConfigRepository Configs { get; }
    IRoleByDonViRepository roleByDonVis { get; }
    IRole_DV_PBRepository role_DV_PBs { get; }
    IChiTiet_DV_PB_BPRepository chiTiet_DV_PB_BPs { get; }
    INhomDoiTacRepository NhomDoiTacs { get; }
    ITaiXeRepository TaiXes { get; }
    IAdsunRepository Adsuns { get; }
    IDS_PhuongTienRepository DS_PhuongTiens { get; }
    IPhuongTien_PhuTrachRepository PhuongTien_PhuTrachs { get; }
    IPhuongTien_DonViRepository PhuongTien_DonVis { get; }
    IDS_ThietBiRepository DS_Thietbis { get; }
    IDM_DonViRepository DM_DonVis { get; }
    IDM_BoPhanRepository DM_BoPhans { get; }
    IDM_NhomRepository DM_Nhoms { get; }
    IDM_LoaiRepository DM_Loais { get; }
    IDM_ModelRepository DM_Models { get; }
    IDM_TinhTrangRepository DM_TinhTrangs { get; }
    ITranslateRepository Translates { get; }
    IStatusRepository Statuss { get; }



    int Complete();
  }
}
