using ERP.Infrastructure;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using static ERP.Data.MyDbContext;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Presentation;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using System.Security.Claims;
using ERP.Helpers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading;
using static ERP.Commons;
using Microsoft.Extensions.Configuration;
using ThacoLibs;
using System.Data;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_DS_PhuongTienController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        private readonly PushThongBao _thongbao;
        private readonly DbAdapter dbAdapter;
        private readonly IConfiguration configuration;
        private readonly HttpClient _httpClient;
        private static readonly string Url_push = "https://fcm.googleapis.com/v1/projects/bms-mobi/messages:send";
        public MMS_DS_PhuongTienController(IConfiguration _configuration, IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment,
        PushThongBao thongbao, DataService master, HttpClient httpClient)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
            _thongbao = thongbao;
            _httpClient = httpClient;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            dbAdapter = new DbAdapter(connectionString);
        }

        [HttpGet]
        public ActionResult Get(string keyword, int page = 1)
        {
            if (keyword == null) keyword = "";
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();

            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
                ).Select(x => new
                {
                    x.Id,
                    x.BienSo1,
                    x.BienSo2,
                    x.DonViSuDung,
                    x.HinhAnh,
                    x.TinhTrang,
                });
            if (page == -1)
            {
                return Ok(data);
            }
            else
            {
                int totalRow = data.Count();
                int pageSize = pageSizeData[0].PageSize;
                int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);

                // Kiểm tra và điều chỉnh giá trị của page
                if (page < 1)
                {
                    page = 1;
                }
                else if (page > totalPage)
                {
                    page = totalPage;
                }

                var datalist = data.Skip((page - 1) * pageSize).Take(pageSize);
                return Ok(new
                {
                    totalRow,
                    totalPage,
                    pageSize,
                    datalist
                });
            }
        }

        [HttpGet("GetListPhuongTienTheoCaNhan")]
        public ActionResult GetListPhuongTienTheoCaNhan(Guid? User_Id, string keyword = null)
        {
            // var model = uow.DM_Models.GetAll(x => !x.IsDeleted);
            string[] include = { "HangMuc" };
            var thongtinhangmuc = uow.ThongTinTheoHangMucs.GetAll(x => !x.IsDeleted && x.HangMuc.GhiChu != "Kiểm tra thường xuyên", null, include);
            var listTinhTrang = uow.DM_TinhTrangs.GetAll(x => !x.IsDeleted, x => x.OrderByDescending(x => x.Arrange));
            string[] includes = { "PhuongTien", "PhuongTien.DM_Model", "PhuongTien.LichSuBaoDuong", "PhuongTien.DM_Model.DM_TanSuat", "PhuongTien.DM_TinhTrang" };
            var data = uow.MMS_PhuTrachBoPhans.GetAll(t => !t.IsDeleted
            && (t.User_Id == User_Id || t.User2_Id == User_Id), t => t.OrderBy(x => x.PhuongTien.DM_TinhTrang.Arrange), includes
                ).Select(x =>
                {
                    // var dinhmuc = model.Where(t => t.Id == x.PhuongTien.Model_Id)?.FirstOrDefault()?.GiaTri;
                    string tinhTrang = null; // Giá trị mặc định
                    // bool isDenHan = false;

                    // if (dinhmuc > 0 && x.PhuongTien?.SoKM_Adsun > dinhmuc)
                    // {
                    //     isDenHan = true;
                    // }
                    var thongTin = thongtinhangmuc.Where(t => t.PhuongTien_Id == x.PhuongTien_Id).ToList();
                    bool isDenHan = false;
                    foreach (var tt in thongTin)
                    {
                        var dinhmuc = tt.HangMuc?.DinhMuc ?? 0;
                        var giaTri = tt.GiaTriBaoDuong;
                        var canhbao = tt.HangMuc?.CanhBao_GanDenHan ?? 0;

                        var ngay = tt.PhuongTien?.LichSuBaoDuong?.NgayHoanThanh;
                        var now = DateTime.Now;

                        if (dinhmuc > 0)
                        {
                            var chenhLech = dinhmuc - ((x.PhuongTien?.SoKM ?? 0) - giaTri);
                            var chenhLechAdsun = dinhmuc - ((x.PhuongTien?.SoKM_Adsun ?? 0) - giaTri);
                            // var chenhLechngay = dinhmuc - ((now - ngay)?.Days ?? 0);
                            if (chenhLech <= canhbao || chenhLechAdsun <= canhbao)
                            {
                                isDenHan = true; // Nếu có ít nhất một hạng mục vượt giới hạn, gán isDenHan = true
                            }
                        }
                    }

                    // if (x.PhuongTien?.LichSuBaoDuong != null && isDenHan)
                    // {
                    //     tinhTrang = x.PhuongTien.LichSuBaoDuong.IsHoanThanh ? "Đã hoàn thành bảo dưỡng" :
                    //                 (x.PhuongTien.LichSuBaoDuong.IsBaoDuong && x.PhuongTien.LichSuBaoDuong.IsYeuCau && x.PhuongTien.LichSuBaoDuong.IsDuyet && !x.PhuongTien.LichSuBaoDuong.IsHoanThanh) ? "Đang bảo dưỡng" :
                    //                 (x.PhuongTien.LichSuBaoDuong.IsYeuCau && !x.PhuongTien.LichSuBaoDuong.IsBaoDuong && !x.PhuongTien.LichSuBaoDuong.IsDuyet && !x.PhuongTien.LichSuBaoDuong.IsHoanThanh) ? "Đã được yêu cầu bảo dưỡng" :
                    //                 (x.PhuongTien.LichSuBaoDuong.IsDuyet && x.PhuongTien.LichSuBaoDuong.IsYeuCau && !x.PhuongTien.LichSuBaoDuong.IsBaoDuong && !x.PhuongTien.LichSuBaoDuong.IsHoanThanh) ? "Đã được duyệt bảo dưỡng" :
                    //                 "Đang hoạt động";
                    // }
                    // tinhTrang = tinhTrang == null ? (isDenHan ? "Đã đến hạn bảo dưỡng" : "Đang hoạt động") : tinhTrang;

                    return new
                    {
                        key = x.Id,
                        Id = x.PhuongTien_Id,
                        x.PhuongTien?.Model_Id,
                        Model = x.PhuongTien?.DM_Model?.Name,
                        Model_Option = x.PhuongTien?.DM_Model?.Option,
                        x.PhuongTien?.DM_Model?.DM_TanSuat?.GiaTri,
                        x.PhuongTien?.BienSo1,
                        x.PhuongTien?.SoKhung,
                        x.PhuongTien?.LoaiPT_Id,
                        x.PhuongTien?.TinhTrang_Id,
                        x.PhuongTien?.BienSo2,
                        x.BoPhan_Id,
                        x.PhuongTien?.MaPhuongTien,

                        // IsYeuCau = x.PhuongTien?.LichSuBaoDuong != null && !x.PhuongTien.LichSuBaoDuong.IsHoanThanh ? x.PhuongTien?.LichSuBaoDuong?.IsYeuCau : false,
                        IsYeuCau = x.PhuongTien?.DM_TinhTrang?.Arrange == "11" ? false : true,
                        SoKM = x.PhuongTien?.SoKM.ToString(),
                        x?.PhuongTien?.SoChuyenXe,
                        SoKM_Adsun = x?.PhuongTien?.SoKM_Adsun.ToString(),
                        NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),
                        // IsDenHan = (dinhmuc > 0 && x.PhuongTien?.SoKM_Adsun > dinhmuc) ? true : false,
                        IsDenHan = isDenHan,
                        // TinhTrang = tinhTrang,
                        TinhTrang = x.PhuongTien?.DM_TinhTrang?.Name
                    };
                })
                .GroupBy(x => x.BienSo1)  // Nhóm theo biển số
    .Select(g => g.FirstOrDefault()) // Chỉ lấy một phần tử đầu tiên của mỗi nhóm
        .OrderBy(x => x.TinhTrang) // Đưa các bản ghi chưa yêu cầu lên trước
    .ThenBy(x => x.IsYeuCau)
    .ToList();
            return Ok(data);
        }

        [HttpPost("PushThongBao")]
        public async Task SendPushNotificationAsync(string body, string NguoiYeuCau, List<Guid?> listIds)
        {
            var claims = User.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var dataList = _master.GetListToken();
            var tokensToSend = new List<string>();
            var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == Guid.Parse("ac9ead22-e0e7-488c-92dc-7dbdf180e027"))
            .Select(x => x.FCMToken)
            .ToList();
            tokensToSend.AddRange(relatedTokens);


            // Lấy Access Token
            var accessToken = await _thongbao.GetAccessTokenAsync();
            var tokensToSendDistinct = tokensToSend.Distinct().ToList();
            foreach (var token in tokensToSendDistinct)
            {
                // Tạo HTTP Request
                var user = dataList.Single(x => !x.IsDeleted && x.FCMToken == token);

                var fullName = user != null ? user?.NguoiPhuTrach.Split('-')[0].Trim() : "";
                // Payload JSON
                var payload = new
                {
                    message = new
                    {
                        token = token,
                        notification = new
                        {
                            title = $"Xin chào, {fullName}",
                            body = body
                        },
                        data = new
                        {
                            type = "xac_nhan",
                            listIds = JsonConvert.SerializeObject(listIds) // Chuyển List<Guid?> thành JSON string
                        }
                    }
                };
                var request = new HttpRequestMessage(HttpMethod.Post, Url_push)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                };
                var response = await _httpClient.SendAsync(request);

                // Xử lý kết quả
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Notification sent successfully!");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error sending notification: {response.StatusCode} - {error}");
                }
            }
        }

        [HttpPost("New")]
        public async Task SendPushNotificationNewAsync(string body, string NguoiYeuCau, Guid? PhuongTien_Id)
        {
            var dataList = _master.GetListToken();
            var tokensToSend = new List<string>();
            if (!body.Contains("Hệ thống"))
            {
                // var claims = User.Claims;
                // var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var NguoiPhuTrach = uow.MMS_PhuTrachBoPhans.GetSingle(x => !x.IsDeleted && x.PhuongTien_Id == PhuongTien_Id);

                if (NguoiPhuTrach != null && NguoiPhuTrach.User_Id != null)
                {
                    var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == Guid.Parse("ac9ead22-e0e7-488c-92dc-7dbdf180e027"))
            // var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == NguoiPhuTrach.User_Id)
            .Select(x => x.FCMToken)
            .ToList();
                    tokensToSend.AddRange(relatedTokens);
                }
            }
            else
            {
                var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == Guid.Parse("ac9ead22-e0e7-488c-92dc-7dbdf180e027"))
           .Select(x => x.FCMToken)
           .ToList();
                tokensToSend.AddRange(relatedTokens);
            }
            // Lấy Access Token
            var accessToken = await _thongbao.GetAccessTokenAsync();
            var tokensToSendDistinct = tokensToSend.Distinct().ToList();
            foreach (var token in tokensToSendDistinct)
            {
                // Tạo HTTP Request
                var user = dataList.Single(x => !x.IsDeleted && x.FCMToken == token);

                var fullName = user != null ? user?.NguoiPhuTrach.Split('-')[0].Trim() : "";
                // Payload JSON
                var payload = new
                {
                    message = new
                    {
                        token = token,
                        notification = new
                        {
                            title = $"Xin chào, {fullName}",
                            body = body
                        },
                        data = new
                        {
                            type = "xac_nhan",
                            listIds = PhuongTien_Id?.ToString() // Chuyển List<Guid?> thành JSON string
                        }
                    }
                };
                var request = new HttpRequestMessage(HttpMethod.Post, Url_push)
                {
                    Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                };
                var response = await _httpClient.SendAsync(request);

                // Xử lý kết quả
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Notification sent successfully!");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error sending notification: {response.StatusCode} - {error}");
                }
            }
        }

        [HttpGet("DSTheoBoPhan")]
        public async Task<ActionResult> Get(Guid? BoPhan_Id)
        {
            var dataDonVi = await _master.GetDonVi();
            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted && (BoPhan_Id == null || t.BoPhan_Id == BoPhan_Id)
           , t => t.OrderByDescending(x => x.NgayBatDau)
                ).Select(x => new
                {
                    x.Id,
                    x.BienSo1,
                    x.SoKhung,
                    x.Model_Id,
                    x.DonVi_Id,
                    x.LoaiPT_Id,
                    x.TinhTrang_Id,
                    x.BienSo2,
                    DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                    x.BoPhan_Id,
                    x.KLBT,
                    x.KLHH,
                    x.KLKT,
                    x.KLTB,
                    x.MaPhuongTien,
                    x.SoKM,
                    x.SoChuyenXe,
                    x.SoKM_Adsun,
                    NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),

                });

            return Ok(data);
        }

        [HttpGet("All")]
        public async Task<ActionResult> GetAll(Guid? DonVi_Id, Guid? TinhTrang_Id, string keyword = null)
        {
            string[] includes = { "DM_Model", "DM_Loai", "DM_TinhTrang", "LichSuBaoDuong", "PhuTrachBoPhan" };
            string[] include = { "HangMuc" };
            var dataDonVi = await _master.GetDonVi();
            var thongtinhangmuc = uow.ThongTinTheoHangMucs.GetAll(x => !x.IsDeleted, null, include);
            // var dataBoPhan = await _master.GetBoPhan();
            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
            && (DonVi_Id == null || t.DonVi_Id == DonVi_Id)
            && (TinhTrang_Id == null || t.TinhTrang_Id == TinhTrang_Id)
            && (string.IsNullOrEmpty(keyword) || t.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.BienSo2.ToLower().Contains(keyword.ToLower())
            || t.PhuTrachBoPhan.NhanVien.ToLower().Contains(keyword.ToLower())
            || t.PhuTrachBoPhan.MaNhanVien.ToLower().Contains(keyword.ToLower())
            ), t => t.OrderBy(x => x.DM_TinhTrang.Arrange), includes
                ).Select(x =>
                {
                    // var dinhmuc = model.Where(t => t.Id == x.Model_Id)?.FirstOrDefault()?.DM_TanSuat?.GiaTri;
                    // int dinhmucValue = 0;
                    // if (!string.IsNullOrEmpty(dinhmuc) && int.TryParse(dinhmuc, out int parsedValue))
                    // {
                    //     dinhmucValue = parsedValue;
                    // }
                    // bool isDenHan = (dinhmucValue > 0 && (dinhmucValue - (x?.SoKM_Adsun ?? 0) <= Commons.GioiHan));

                    var thongTin = thongtinhangmuc.Where(t => t.PhuongTien_Id == x.Id).ToList();
                    bool isDenHan = false;
                    foreach (var tt in thongTin)
                    {
                        var dinhmuc = tt.HangMuc?.DinhMuc ?? 0;
                        var giaTri = tt.GiaTriBaoDuong;
                        var canhbao = tt.HangMuc?.CanhBao_GanDenHan ?? 0;
                        // var ngay = tt.PhuongTien?.LichSuBaoDuong?.NgayHoanThanh;
                        // var now = DateTime.Now;

                        if (dinhmuc > 0)
                        {
                            var chenhLech = dinhmuc - ((x?.SoKM ?? 0) - giaTri);
                            var chenhLechAdsun = dinhmuc - ((x?.SoKM_Adsun ?? 0) - giaTri);
                            // var chenhLechngay = dinhmuc - ((now - ngay)?.Days ?? 0);

                            if (chenhLech <= canhbao || chenhLechAdsun <= canhbao)
                            {
                                isDenHan = true; // Nếu có ít nhất một hạng mục vượt giới hạn, gán isDenHan = true
                            }
                        }
                    }
                    // bool isYeuCau = x.LichSuBaoDuong != null && !x.LichSuBaoDuong.IsHoanThanh ? x.LichSuBaoDuong.IsYeuCau : false;
                    bool isYeuCau = (x.DM_TinhTrang?.Arrange == "11" || x.DM_TinhTrang?.Arrange == "6") ? false : true;

                    return new
                    {
                        x.Id,
                        Model = x.DM_Model?.Name,
                        Model_Option = x.DM_Model?.Option,
                        x.BienSo1,
                        x.SoKhung,
                        x.Model_Id,
                        x.DonVi_Id,
                        x.LoaiPT_Id,
                        x.TinhTrang_Id,
                        x.BienSo2,
                        x.BoPhan_Id,
                        x.LichSuBaoDuong?.NguoiYeuCau,
                        SoKM_NgayBaoDuong = x.SoKM_NgayBaoDuong.ToString(),
                        SoKM_NgayBaoDuong2 = x.SoKM_NgayBaoDuong,
                        DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                        // NguoiPhuTrach = lichsu.Where(t => t.PhuongTien_Id == x.Id)?.FirstOrDefault()?.NhanVien,
                        // TenBoPhan = dataBoPhan.Where(t => !t.IsDeleted && t.Id == x.BoPhan_Id)?.FirstOrDefault()?.TenPhongBan,
                        x.KLBT,
                        x.KLHH,
                        x.KLKT,
                        x.KLTB,
                        x.LichSuBaoDuong_Id,
                        x.LichSuBaoDuong?.GhiChu,
                        x.LichSuBaoDuong?.DiaDiem_Id,
                        // TinhTrang = !isYeuCau && isDenHan ? "Đã đến hạn bảo dưỡng" : x.DM_TinhTrang?.Name,
                        TinhTrang = x.DM_TinhTrang?.Name,
                        x.DM_TinhTrang?.Arrange,
                        NguoiPhuTrach = x.PhuTrachBoPhan?.NhanVien,
                        x.PhuTrachBoPhan?.MaNhanVien,
                        x.Note,
                        x.Address_nearest,
                        LoaiPT = x.DM_Loai?.Name,
                        x.MaPhuongTien,
                        SoKM = x.SoKM_NgayBaoDuong.ToString(),
                        SoKM2 = x.SoKM,
                        x?.LichSuBaoDuong?.IsDuyet,
                        x.LichSuBaoDuong?.IsHoanThanh,
                        IsBaoDuong = x.LichSuBaoDuong_Id != null && !x.LichSuBaoDuong.IsDeleted && !x.LichSuBaoDuong.IsHoanThanh ? x.LichSuBaoDuong?.IsBaoDuong : null,
                        IsLenhHoanThanh = x.LichSuBaoDuong_Id != null && !x.LichSuBaoDuong.IsDeleted && !x.LichSuBaoDuong.IsHoanThanh ? x.LichSuBaoDuong?.IsLenhHoanThanh : null,
                        SoKM_Adsun = x.SoKM_Adsun.ToString(),
                        SoKM_Adsun2 = x.SoKM_Adsun,
                        NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),
                        NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy}", x.LichSuBaoDuong?.NgayDiBaoDuong),
                        // IsYeuCau = x.LichSuBaoDuong != null ? x.LichSuBaoDuong.IsYeuCau : false,
                        // IsDenHan = (dinhmucValue > 0 && (dinhmucValue - (x?.SoKM_Adsun ?? 0) <= Commons.GioiHan)) ? true : false,
                        IsYeuCau = isYeuCau,
                        IsDenHan = isDenHan,
                    };
                })
                .OrderBy(x => x.TinhTrang) // Đưa các bản ghi chưa yêu cầu lên trước
                .ThenBy(x => x.IsYeuCau);

            return Ok(data);
        }

        [HttpGet("PTT")]
        public async Task<ActionResult> GetAllPhuongTien(Guid? DonVi_Id, Guid? TinhTrang_Id, string keyword = null)
        {
            dbAdapter.connect();
            dbAdapter.createStoredProceder("Get_DS_PhuongTien");
            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = DonVi_Id;
            dbAdapter.sqlCommand.Parameters.Add("@TinhTrang_Id", SqlDbType.UniqueIdentifier).Value = TinhTrang_Id;
            dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
            var result = dbAdapter.runStored2ObjectList();
            dbAdapter.deConnect();
            // var json = JsonConvert.SerializeObject(result);
            // var resultList = JsonConvert.DeserializeObject<List<PhuongTienDTO>>(json);
            // var model = uow.DM_Models.GetAll(x => !x.IsDeleted);
            // foreach (var item in resultList)
            // {
            //     var dinhmuc = model.Where(x => x.Id == item.Model_Id)?.FirstOrDefault()?.GiaTri;
            //     if (dinhmuc > 0)
            //     {
            //         var chenhLech = dinhmuc - item?.SoKM_Adsun;
            //         if (chenhLech <= 20000)
            //         {
            //             var noidung = $"Phương tiện {item.BienSo1} (Model: {item.Model} - {item.Model_Option}) vượt định mức {dinhmuc} KM.";
            //             await SendPushNotificationNewAsync(
            //             body: noidung,
            //             NguoiYeuCau: "Hệ thống",
            //             item.Id
            //             );
            //         }
            //     }
            // }
            return Ok(result);
        }


        [HttpGet("PT")]
        public async Task<ActionResult> GetPT(Guid? DonVi_Id, string keyword = null)
        {
            var dataDonVi = await _master.GetDonVi();
            // var dataBoPhan = await _master.GetBoPhan();
            string[] includes = { "DM_Model", "DM_Loai", "DM_TinhTrang" };
            string[] include = { "HangMuc" };
            var thongtinhangmuc = uow.ThongTinTheoHangMucs.GetAll(x => !x.IsDeleted && x.HangMuc.GhiChu != "Kiểm tra thường xuyên", null, include);
            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
            && (DonVi_Id == null || t.DonVi_Id == DonVi_Id)
            && (string.IsNullOrEmpty(keyword) || t.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.BienSo2.ToLower().Contains(keyword.ToLower())), t => t.OrderByDescending(x => x.NgayBatDau), includes
                ).Select(x => new
                {
                    x.Id,
                    Model = x.DM_Model?.Name,
                    Model_Option = x.DM_Model?.Option,
                    x.BienSo1,
                    x.SoKhung,
                    x.Model_Id,
                    x.DonVi_Id,
                    x.LoaiPT_Id,
                    x.TinhTrang_Id,
                    x.BienSo2,
                    x.BoPhan_Id,
                    DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                    // TenBoPhan = dataBoPhan.Where(t => !t.IsDeleted && t.Id == x.BoPhan_Id)?.FirstOrDefault()?.TenPhongBan,
                    x.KLBT,
                    x.KLHH,
                    x.KLKT,
                    x.KLTB,
                    TinhTrang = x.DM_TinhTrang?.Name,
                    x.Note,
                    x.Address_nearest,
                    LoaiPT = x.DM_Loai?.Name,
                    x.MaPhuongTien,
                    x.SoKM,
                    x.SoChuyenXe,
                    x.SoKM_Adsun,
                    x.SoKM_NgayBaoDuong,
                    NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),

                });
            List<string> danhSachXeDenHan = new List<string>();
            foreach (var item in data)
            {
                var thongTin = thongtinhangmuc
        .Where(x => x.PhuongTien_Id == item.Id)
        .ToList(); // Lọc tất cả thông tin liên quan đến PhuongTien_Id
                List<string> danhSachHangMucVuot = new List<string>();
                foreach (var tt in thongTin)
                {
                    var dinhmuc = tt.HangMuc?.DinhMuc ?? 0;
                    var giaTri = tt.GiaTriBaoDuong;
                    var giatri = tt.HangMuc?.CanhBao_GanDenHan ?? 0;
                    // var ngay = tt.PhuongTien?.LichSuBaoDuong?.NgayHoanThanh;
                    // var now = DateTime.Now;
                    if (dinhmuc > 0)
                    {
                        var chenhLech = dinhmuc - ((item?.SoKM ?? 0) - giaTri);
                        var chenhLechAdsun = dinhmuc - ((item?.SoKM_Adsun ?? 0) - giaTri);
                        // var chenhLechngay = dinhmuc - ((now - ngay)?.Days ?? 0);
                        if (chenhLech <= giatri || chenhLechAdsun <= giatri)
                        {
                            danhSachHangMucVuot.Add(tt.HangMuc?.NoiDungBaoDuong); // Thêm hạng mục vi phạm vào danh sách
                        }
                    }
                }
                if (danhSachHangMucVuot.Count > 0)
                {
                    string[] includess = { "LichSuBaoDuong" };
                    var phuongtien = uow.DS_PhuongTiens.GetSingle(x => !x.IsDeleted && x.Id == item.Id, includess);
                    if (phuongtien != null
                    && ((phuongtien.LichSuBaoDuong_Id != null && phuongtien.LichSuBaoDuong.IsHoanThanh)
                    || phuongtien.LichSuBaoDuong_Id == null))
                    {
                        phuongtien.TinhTrang_Id = Guid.Parse("90b5fae3-8fb6-43e3-8af2-6cec3627d3fc");
                        uow.DS_PhuongTiens.Update(phuongtien);
                        danhSachXeDenHan.Add(item.BienSo1);
                        // // Gộp danh sách hạng mục thành 1 thông báo duy nhất
                        // var danhSachText = string.Join(", ", danhSachHangMucVuot);
                        var noidung = $"Phương tiện {item.BienSo1} (Model: {item.Model} - {item.Model_Option}) có {danhSachHangMucVuot.Count} hạng mục đến hạn bảo dưỡng.";
                        await SendPushNotificationNewAsync(
                            body: noidung,
                            NguoiYeuCau: "Hệ thống",
                            item.Id
                        );
                    }
                }
            }

            if (danhSachXeDenHan.Count > 0)
            {
                var noidungQuanLy = $"Hệ thống: Có {danhSachXeDenHan.Count} phương tiện đến hạn bảo dưỡng.";
                await SendPushNotificationNewAsync(
                    body: noidungQuanLy,
                    NguoiYeuCau: "Hệ thống",
                    Guid.Empty // ID của quản lý hoặc để rỗng
                );
            }
            uow.Complete();
            return Ok(data);
        }

        [HttpGet("List")]
        public ActionResult GetList(Guid? PhuongTien_Id, string keyword = null)
        {
            string[] includes = { "DM_Model", "DM_Loai", "DM_TinhTrang" };
            // var lichsu = uow.LichSuBaoDuongs.GetAll(x => !x.IsDeleted && x.TrangThai == null).OrderByDescending(x => x.Ngay);
            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
            && (PhuongTien_Id == null || t.Id == PhuongTien_Id)
            && (string.IsNullOrEmpty(keyword) || t.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.BienSo2.ToLower().Contains(keyword.ToLower())), t => t.OrderByDescending(x => x.NgayBatDau), includes
                ).Select(x => new
                {
                    x.Id,
                    Model = x.DM_Model?.Name,
                    Model_Option = x.DM_Model?.Option,
                    GiaTri = x.DM_Model?.GiaTri.ToString(),
                    x.BienSo1,
                    x.SoKhung,
                    x.Model_Id,
                    x.DonVi_Id,
                    x.LoaiPT_Id,
                    x.TinhTrang_Id,
                    x.BienSo2,
                    x.BoPhan_Id,
                    x.KLBT,
                    x.KLHH,
                    x.KLKT,
                    x.KLTB,
                    TinhTrang = x.DM_TinhTrang?.Name,
                    x.Note,
                    x.Address_nearest,
                    LoaiPT = x.DM_Loai?.Name,
                    x.MaPhuongTien,
                    SoKM = x.SoKM.ToString(),
                    // IsYeuCau = lichsu.Where(t => t.PhuongTien_Id == x.Id).Count() > 0 ? true : false,
                    IsYeuCau = x?.LichSuBaoDuong != null ? x?.LichSuBaoDuong?.IsYeuCau : false,
                    x.SoChuyenXe,
                    SoKM_Adsun = x.SoKM_Adsun.ToString(),
                    NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),

                });
            return Ok(data);
        }

        [HttpGet("GetListCanBaoDuongTheoCaNhan")]
        public ActionResult GetListCanBaoDuongTheoCaNhan(Guid? User_Id, string keyword = null)
        {
            string[] includeds = { "DM_TanSuat" };
            var model = uow.DM_Models.GetAll(x => !x.IsDeleted, null, includeds);
            // var lichsu = uow.LichSuBaoDuongs.GetAll(x => !x.IsDeleted && x.TrangThai == null).OrderByDescending(x => x.Ngay);
            string[] includes = { "PhuongTien", "PhuongTien.DM_Model", "PhuongTien.DM_Loai", "PhuongTien.DM_TinhTrang", "PhuongTien.LichSuBaoDuong" };
            var data = uow.MMS_PhuTrachBoPhans.GetAll(t => !t.IsDeleted
            && t.User_Id == User_Id
            && (string.IsNullOrEmpty(keyword) || t.PhuongTien.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.PhuongTien.BienSo2.ToLower().Contains(keyword.ToLower())), t => t.OrderByDescending(x => x.NgayBatDau), includes
                )
                .GroupBy(x => x.PhuongTien_Id) // Nhóm theo PhuongTien_Id để loại bỏ trùng lặp
                .Select(g => g.FirstOrDefault())
                .Select(x =>
                {
                    var dinhmuc = model.Where(t => t.Id == x.PhuongTien.Model_Id)?.FirstOrDefault()?.DM_TanSuat?.GiaTri;
                    return new
                    {
                        key = x.Id,
                        Id = x.PhuongTien_Id,
                        Model = x.PhuongTien?.DM_Model?.Name,
                        Model_Option = x.PhuongTien?.DM_Model?.Option,
                        GiaTri = x.PhuongTien?.DM_Model?.GiaTri.ToString(),
                        x.PhuongTien?.BienSo1,
                        x.PhuongTien?.SoKhung,
                        x.PhuongTien?.Model_Id,
                        x.DonVi_Id,
                        x.PhuongTien?.LoaiPT_Id,
                        x.PhuongTien?.TinhTrang_Id,
                        x.PhuongTien?.BienSo2,
                        x.BoPhan_Id,
                        x.PhuongTien?.MaPhuongTien,
                        SoKM = x.PhuongTien?.SoKM.ToString(),
                        // IsYeuCau = lichsu.Where(t => t.PhuongTien_Id == x.PhuongTien_Id).Count() > 0 ? true : false,
                        x?.PhuongTien?.SoChuyenXe,
                        IsDenHan = (int.Parse(dinhmuc) > 0 && x.PhuongTien?.SoKM_Adsun > int.Parse(dinhmuc)) ? true : false,
                        SoKM_Adsun = x?.PhuongTien?.SoKM_Adsun.ToString(),
                        NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),

                    };
                });
            return Ok(data);
        }


        [HttpGet("GetById")]
        public async Task<ActionResult> Get(Guid id)
        {
            var adsun = new Adsun_DTO();
            var dataDonVi = await _master.GetDonVi();
            // var lichsu = uow.MMS_PhuTrachBoPhans.GetAll(x => !x.IsDeleted, x => x.OrderByDescending(x => x.CreatedDate));
            // HttpClient httpClient = new HttpClient();
            // VehicleService vehicleService = new VehicleService(httpClient);
            string[] includes = { "DM_Model", "DM_Loai", "DM_TinhTrang", "PhuTrachBoPhan", "DM_Model.DM_TanSuat" };
            var query = uow.DS_PhuongTiens.GetAll(x => x.Id == id, null, includes).Select(x => new
            {
                x.Id,
                Model = x.DM_Model?.Name,
                Model_Option = x.DM_Model?.Option,
                x.DM_Model?.DM_TanSuat?.GiaTri,
                x.SoKhung,
                x.BienSo1,
                x.Model_Id,
                x.DonVi_Id,
                x.LoaiPT_Id,
                x.TinhTrang_Id,
                x.BienSo2,
                DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                NguoiPhuTrach = x.PhuTrachBoPhan?.NhanVien,
                x.PhuTrachBoPhan?.HinhAnh_NhanVien,
                x.PhuTrachBoPhan?.MaNhanVien,
                SoKMTuNgayBaoDuong = x.SoKM - x.SoKM_NgayBaoDuong,
                x.SoKM_NgayBaoDuong,
                x.KLBT,
                x.KLHH,
                x.KLKT,
                x.KLTB,
                // TinhTrang = x.DM_TinhTrang?.Name,
                TinhTrang = ((x.LichSuBaoDuong_Id == null &&
                                 x.DM_Model?.DM_TanSuat?.GiaTri != null &&
                                 int.TryParse(x.DM_Model?.DM_TanSuat?.GiaTri, out int dinhmuc2) &&
                                 (dinhmuc2 - x.SoKM < 200))
                                ? "Đã đến hạn bảo dưỡng"
                                : x.DM_TinhTrang?.Name),
                x.Note,
                x.Address_nearest,
                LoaiPT = x.DM_Loai?.Name,
                x.MaPhuongTien,
                x.SoKM,
                x.SoKM_Adsun,
                x.SoChuyenXe,
                x.NgayBatDau
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(query.BienSo1))
            {
                HttpClient httpClient = new HttpClient();
                VehicleService vehicleService = new VehicleService(httpClient);
                VehicleInfo vehicle = await vehicleService.GetVehicleByPlateAsync(query.BienSo1.Replace("-", "").Replace(".", ""));
                if (vehicle != null)
                {
                    //     ToaDo = $"{vehicle.Lat},{vehicle.Lng}";
                    // }
                    adsun.Plate = vehicle.Plate;
                    adsun.ToaDo = $"{vehicle.Lat},{vehicle.Lng}" ?? "0,0";
                    adsun.Km = vehicle.Km;
                    adsun.Speed = vehicle.Speed;
                    adsun.GroupName = vehicle.GroupName;
                    adsun.Address = vehicle.Address;
                    adsun.Angle = vehicle.Angle;
                }
            }

            return Ok(new { query, adsun });
        }

        [HttpGet("Adsun")]
        public ActionResult Adsun(string keyword)
        {
            var claims = User.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var maNhanVien = claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            var data = uow.Adsuns.GetAll(t => !t.IsDeleted).Select(x => new
            {
                x.Id,
                x.Plate,
                x.GroupName
            });
            return Ok(data);
        }

        [HttpPost("Read_Excel")]
        public async Task<ActionResult> Read_Excel(IFormFile file)
        {

            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = DateTime.Now;
            // Rename file
            string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
            string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            string[] supportedTypes = new[] { "xls", "xlsx" };
            if (supportedTypes.Contains(fileExt))
            {
                var dataList = await _master.GetDonVi();
                var DonVi = uow.DM_DonVis.GetAll(x => !x.IsDeleted);
                var Tinhtrang = uow.DM_TinhTrangs.GetAll(x => !x.IsDeleted);
                var loaiPT = uow.DM_Loais.GetAll(x => !x.IsDeleted);
                var model = uow.DM_Models.GetAll(x => !x.IsDeleted);
                string webRootPath = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                string fullPath = Path.Combine(webRootPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                byte[] file_byte = System.IO.File.ReadAllBytes(fullPath);
                //Kiểm tra tồn tại file và xóa
                System.IO.File.Delete(fullPath);
                using (MemoryStream ms = new MemoryStream(file_byte))
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_DS_PhuongTien>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }

                        object MaPT = worksheet.Cells[i, 1].Value;
                        object BienSo1 = worksheet.Cells[i, 2].Value;
                        object BienSo2 = worksheet.Cells[i, 3].Value;

                        object LoaiPT = worksheet.Cells[i, 4].Value;
                        object Model = worksheet.Cells[i, 5].Value;
                        object ModelOption = worksheet.Cells[i, 6].Value;
                        object DonViSuDung = worksheet.Cells[i, 7].Value;

                        object KLBT = worksheet.Cells[i, 8].Value;
                        object KLHH = worksheet.Cells[i, 9].Value;
                        object KLTB = worksheet.Cells[i, 10].Value;
                        object KLKT = worksheet.Cells[i, 11].Value;
                        object Address_nearest = worksheet.Cells[i, 12].Value;
                        object TinhTrang = worksheet.Cells[i, 13].Value;
                        object NgayBatDau = worksheet.Cells[i, 14].Value;
                        object ViTri = worksheet.Cells[i, 15].Value;
                        object Lat = worksheet.Cells[i, 16].Value;
                        object Long = worksheet.Cells[i, 17].Value;


                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DS_PhuongTien();
                        info.Id = Guid.NewGuid();
                        info.MaPhuongTien = MaPT?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.BienSo1 = BienSo1?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.BienSo2 = BienSo2?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.DonViSuDung = DonViSuDung?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TinhTrang = TinhTrang?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.LoaiPT = LoaiPT?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Model = Model?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Model_Option = ModelOption?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLBT = KLBT?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLHH = KLHH?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLTB = KLTB?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLKT = KLKT?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Address_nearest = Address_nearest?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ViTri = ViTri?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ViTri_Lat = Lat?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ViTri_Long = Long?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";



                        DateTime ConvertFromExcelDate(int excelDate, DateTime baseDate)
                        {
                            return baseDate.AddDays(excelDate - 2);
                        }
                        // Hàm xử lý chuỗi ngày
                        string ProcessDateString(string dateString, DateTime baseDate)
                        {
                            if (dateString == "#N/A" || string.IsNullOrWhiteSpace(dateString))
                            {
                                return "";
                            }
                            if (int.TryParse(dateString, out int excelDate))
                            {
                                DateTime date = ConvertFromExcelDate(excelDate, baseDate);
                                return date.ToString("dd/MM/yyyy");
                            }
                            if (DateTime.TryParse(dateString, out DateTime parsedDate))
                            {
                                return parsedDate.ToString("dd/MM/yyyy");
                            }

                            // Xử lý khi ngày không hợp lệ
                            info.IsLoi = true;
                            lst_Lois.Add($"Lỗi định dạng ngày không hợp lệ {dateString}");
                            return "";
                        }

                        var ngayBatDau = NgayBatDau?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.NgayBatDau = ProcessDateString(ngayBatDau, baseDate);

                        if (string.IsNullOrEmpty(info.MaPhuongTien))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Mã phương tiện không được để trống");
                        }
                        if (string.IsNullOrEmpty(info.DonViSuDung))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Đơn vị không được để trống");
                        }
                        else
                        {
                            var info_DonVi = dataList.Where(x => !x.IsDeleted && x.TenDonVi.ToLower() == info.DonViSuDung.ToLower()).FirstOrDefault();
                            if (info_DonVi == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo đơn vị trong danh mục Đơn Vị");
                            }
                            else
                            {
                                info.DonVi_Id = info_DonVi.Id;

                            }

                        }
                        if (string.IsNullOrEmpty(info.TinhTrang))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Tình trạng không được để trống");
                        }
                        else
                        {
                            var info_TinhTrang = Tinhtrang.Where(x => x.Name.ToLower() == info.TinhTrang.ToLower()).FirstOrDefault();
                            if (info_TinhTrang == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo tình trạng trong danh mục Tình Trạng");
                            }
                            else
                            {
                                info.TinhTrang_Id = info_TinhTrang.Id;

                            }

                        }
                        if (string.IsNullOrEmpty(info.LoaiPT))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Loại phương tiện không được để trống");
                        }
                        else
                        {
                            var info_loaiPT = loaiPT.Where(x => x.Name.ToLower() == info.LoaiPT.ToLower()).FirstOrDefault();
                            if (info_loaiPT == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo loại PT trong danh mục Loại");
                            }
                            else
                            {
                                info.LoaiPT_Id = info_loaiPT.Id;

                            }

                        }
                        if (string.IsNullOrEmpty(info.Model))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Model không được để trống");
                        }
                        else
                        {
                            var info_model = model.Where(x => x.Name.ToLower() == info.Model.ToLower()).FirstOrDefault();
                            if (info_model == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo model trong danh mục");
                            }
                            else
                            {
                                info.Model_Id = info_model.Id;

                            }

                        }

                        // else
                        // {
                        //     var exit = lst_Datas.Where(a => a.SoKhung.ToLower().Trim() == info.SoKhung.ToLower().Trim()).FirstOrDefault();
                        //     if (exit != null)
                        //     {
                        //         info.Id = exit.Id;
                        //     }
                        // }


                        info.lst_Lois = lst_Lois;
                        list_datas.Add(info);
                    }

                    list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();
                    //Kiểm tra trùng dữ liệu khi không có lỗi đầu vào
                    if (list_datas.Where(x => x.IsLoi).Count() == 0)
                    {

                        var processedBienSos = new HashSet<string>();
                        foreach (var item in list_datas)
                        {
                            if (!string.IsNullOrEmpty(item.MaPhuongTien))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.MaPhuongTien == item.MaPhuongTien).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Số Khung {item.MaPhuongTien} bị trùng lặp");
                                }
                            }
                        }
                        list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();
                        return Ok(new { errorCount = list_datas.Where(x => x.IsLoi).Count(), rowCount = list_datas.Count(), list_datas });
                    }
                    else
                    {
                        return Ok(new { errorCount = list_datas.Where(x => x.IsLoi).Count(), rowCount = list_datas.Count(), list_datas });
                    }
                }
            }
            else
                return BadRequest("Định dạng tệp tin không cho phép");

        }
        private int DuplicateSoKhungs(List<ImportMMS_DS_PhuongTien> lst)
        {
            return lst.GroupBy(p => new { p.MaPhuongTien }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DS_PhuongTien> DH)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                IFormatProvider fm = new CultureInfo("en-US", true);

                foreach (var item in DH)
                {

                    DateTime? ngayBatDau = null;
                    if (DateTime.TryParseExact(item.NgayBatDau, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempNgayDenKV))
                    {
                        ngayBatDau = tempNgayDenKV;
                    }

                    var exit = uow.DS_PhuongTiens.GetSingle(x => !x.IsDeleted && x.MaPhuongTien.ToLower() == item.MaPhuongTien.ToLower());
                    if (exit == null)
                    {
                        uow.DS_PhuongTiens.Add(new DS_PhuongTien
                        {
                            MaPhuongTien = item.MaPhuongTien,
                            BienSo1 = item.BienSo1,
                            BienSo2 = item.BienSo2,
                            DonViSuDung = item.DonViSuDung,
                            TinhTrang = item.TinhTrang,
                            NgayBatDau = ngayBatDau,
                            Model_Id = item.Model_Id,
                            TinhTrang_Id = item.TinhTrang_Id,
                            LoaiPT_Id = item.LoaiPT_Id,
                            DonVi_Id = item.DonVi_Id,
                            LoaiPT = item.LoaiPT,
                            Model = item.Model,
                            Model_Option = item.Model_Option,
                            KLBT = item.KLBT,
                            KLHH = item.KLHH,
                            KLTB = item.KLTB,
                            KLKT = item.KLKT,
                            Address_nearest = item.Address_nearest,
                            ViTri = item.ViTri,
                            ViTri_Lat = item.ViTri_Lat,
                            ViTri_Long = item.ViTri_Long,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.MaPhuongTien = item.MaPhuongTien;
                        exit.BienSo1 = item.BienSo1;
                        exit.BienSo2 = item.BienSo2;
                        exit.Model_Id = item.Model_Id;
                        exit.TinhTrang_Id = item.TinhTrang_Id;
                        exit.LoaiPT_Id = item.LoaiPT_Id;
                        exit.DonVi_Id = item.DonVi_Id;
                        exit.DonViSuDung = item.DonViSuDung;
                        exit.TinhTrang = item.TinhTrang;
                        exit.NgayBatDau = ngayBatDau;
                        exit.LoaiPT = item.LoaiPT;
                        exit.Model = item.Model;
                        exit.Model_Option = item.Model_Option;
                        exit.KLBT = item.KLBT;
                        exit.KLHH = item.KLHH;
                        exit.KLTB = item.KLTB;
                        exit.KLKT = item.KLKT;
                        exit.Address_nearest = item.Address_nearest;
                        exit.ViTri = item.ViTri;
                        exit.ViTri_Lat = item.ViTri_Lat;
                        exit.ViTri_Long = item.ViTri_Long;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DS_PhuongTiens.Update(exit);
                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        [HttpPost("UpdateDS")]
        public async Task<ActionResult> Update(Guid? PhuongTien_Id, int SoKM, string TinhTrang, string GhiChu, string NguoiKiemTra, string HinhAnh)
        {
            await _semaphore.WaitAsync();
            try
            {
                // var claims = User.Claims;
                // var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var phuongtien = uow.DS_PhuongTiens.GetById(PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                else
                {
                    phuongtien.UpdatedBy = Guid.Parse(User.Identity.Name);
                    phuongtien.UpdatedDate = DateTime.Now;
                    phuongtien.SoKM = SoKM;
                    uow.DS_PhuongTiens.Update(phuongtien);

                    var LSHangNgay = new LichSuKiemTraHangNgay();
                    LSHangNgay.CreatedDate = DateTime.Now;
                    LSHangNgay.CreatedBy = Guid.Parse(User.Identity.Name);
                    LSHangNgay.PhuongTien_Id = PhuongTien_Id;
                    LSHangNgay.TinhTrang = TinhTrang;
                    LSHangNgay.GhiChu = GhiChu;
                    LSHangNgay.SoKM = SoKM;
                    LSHangNgay.HinhAnh = HinhAnh;
                    LSHangNgay.NguoiKiemTra = NguoiKiemTra;
                    uow.LichSuKiemTraHangNgays.Add(LSHangNgay);
                }
                uow.Complete();
                // await GetPT(phuongtien.DonVi_Id);
                _ = Task.Run(() => GetPT(phuongtien.DonVi_Id)).ContinueWith(t =>
    {
        if (t.IsFaulted)
        {
            Console.WriteLine($"Lỗi trong GetPT: {t.Exception}");
        }
    });

                return Ok();
            }
            finally
            {
                _semaphore.Release(); // Giải phóng quyền truy cập
            }
        }

        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_DS_PhuongTien.xlsx");
            string fileName = "FileMau_DS_PhuongTien_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];


                return Ok(new { data = package.GetAsByteArray(), fileName });
            }
        }


        [HttpPost]
        public ActionResult Post(DS_PhuongTien data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.DS_PhuongTiens.Exists(x => x.MaPhuongTien == data.MaPhuongTien && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaPhuongTien + " đã tồn tại trong hệ thống");
                else if (uow.DS_PhuongTiens.Exists(x => x.MaPhuongTien == data.MaPhuongTien && x.IsDeleted))
                {

                    var d = uow.DS_PhuongTiens.GetAll(x => x.MaPhuongTien == data.MaPhuongTien).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.NgayBatDau = data.NgayBatDau;
                    d.MaPhuongTien = data.MaPhuongTien;
                    d.SoKhung = data.SoKhung;
                    d.SoKM = data.SoKM;
                    d.SoChuyenXe = data.SoChuyenXe;
                    d.BienSo1 = data.BienSo1;
                    d.BienSo2 = data.BienSo2;
                    d.TinhTrang_Id = data.TinhTrang_Id;
                    d.DonVi_Id = data.DonVi_Id;
                    d.LoaiPT_Id = data.LoaiPT_Id;
                    d.Model_Id = data.Model_Id;
                    d.SoKM_Adsun = data.SoKM_Adsun;
                    uow.DS_PhuongTiens.Update(d);

                }
                else
                {
                    DS_PhuongTien cv = new DS_PhuongTien();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.NgayBatDau = data.NgayBatDau;
                    cv.MaPhuongTien = data.MaPhuongTien;
                    cv.SoKhung = data.SoKhung;
                    cv.SoKM = data.SoKM;
                    cv.SoChuyenXe = data.SoChuyenXe;
                    cv.BienSo1 = data.BienSo1;
                    cv.BienSo2 = data.BienSo2;
                    cv.DonVi_Id = data.DonVi_Id;
                    cv.LoaiPT_Id = data.LoaiPT_Id;
                    cv.Model_Id = data.Model_Id;
                    cv.TinhTrang_Id = data.TinhTrang_Id;
                    cv.SoKM_Adsun = data.SoKM_Adsun;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.DS_PhuongTiens.Add(cv);

                    var danhSachHangMuc = uow.DM_HangMucs.GetAll(x => !x.IsDeleted).Select(x => x.Id).ToList();

                    // 🔥 Thêm tất cả hàng mục vào ThongTinTheoHangMuc với PhuongTien_Id vừa thêm
                    foreach (var hangMucId in danhSachHangMuc)
                    {
                        uow.ThongTinTheoHangMucs.Add(new ThongTinTheoHangMuc
                        {
                            PhuongTien_Id = cv.Id,
                            HangMuc_Id = hangMucId,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, DS_PhuongTien duLieu)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (id != duLieu.Id)
                {
                    return BadRequest();
                }
                duLieu.UpdatedBy = Guid.Parse(User.Identity.Name);
                duLieu.UpdatedDate = DateTime.Now;
                uow.DS_PhuongTiens.Update(duLieu);
                uow.Complete();
                //Ghi log truy cập
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        [HttpDelete]
        public ActionResult Delete(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                DS_PhuongTien duLieu = uow.DS_PhuongTiens.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DS_PhuongTiens.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
