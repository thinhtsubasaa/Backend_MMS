using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP.Repositories;

namespace ERP.Infrastructure
{
  public interface IUnitofWork : IDisposable
  {
    ILogRepository Logs { get; }
    IConfigRepository Configs { get; }

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
    IDM_BaoDuongRepository DM_BaoDuongs { get; }
    IKeHoachBaoDuongRepository KeHoachBaoDuongs { get; }
    ILichSuBaoDuongRepository LichSuBaoDuongs { get; }
    IMMS_PhuTrachBoPhanRepository MMS_PhuTrachBoPhans { get; }
    ILichSuPhanBoDonViRepository LichSuPhanBoDonVis { get; }
    IDM_TanSuatRepository DM_TanSuats { get; }
    IGhepNoiPhuongTien_ThietBiRepository GhepNoiPhuongTien_ThietBis { get; }
    IDM_HangMucRepository DM_HangMucs { get; }
    IThongTinTheoHangMucRepository ThongTinTheoHangMucs { get; }
    ILichSuBaoDuong_ChiTietRepository LichSuBaoDuong_ChiTiets { get; }
    ILichSuKiemTraHangNgayRepository LichSuKiemTraHangNgays { get; }
    int Complete();
  }
}
