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
using System.Net.Http;
using ERP.Helpers;
using System.Threading.Tasks;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_QuanLyPhuongTienController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly HttpClient _httpClient;
        public MMS_QuanLyPhuongTienController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, HttpClient httpClient)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _httpClient = httpClient;
        }
        [HttpGet("KiemTraHangNgay")]
        public ActionResult Get(Guid? PhuongTien_Id)
        {
            string[] includes = { "PhuongTien" };
            var data = uow.LichSuKiemTraHangNgays.GetAll(x => !x.IsDeleted && x.PhuongTien_Id == PhuongTien_Id, x => x.OrderByDescending(x => x.CreatedDate), includes)
            .Select(x => new
            {
                x.Id,
                x.PhuongTien?.BienSo1,
                x.PhuongTien?.BienSo2,
                x.GhiChu,
                x.TinhTrang,
                x.HinhAnh,
                x.NguoiKiemTra,
                x.SoKM,
                NgayKiemTra = string.Format("{0:dd/MM/yyyy HH:mm}", x.CreatedDate),
            });
            return Ok(data);
        }

        [HttpGet("GetPhuongTienMobi")]
        public ActionResult GetSoKhungDieuchuyenmobi(string BienSo, string ToaDo)
        {
            var appUser = userManager.FindByIdAsync(User.Identity.Name).Result;

            var data = uow.DS_PhuongTiens.GetAll(x => !x.IsDeleted && (x.BienSo1.ToLower().Contains(BienSo.ToLower())
            || x.BienSo2.ToLower().Contains(BienSo.ToLower())))
            .Select(x => new
            {
                x.Id,
                x.Model,
                x.Model_Option,
                x.BienSo1,
                x.BienSo2,
                x.DonViSuDung,
                x.KLBT,
                x.KLHH,
                x.KLKT,
                x.KLTB,
                x.TinhTrang,
                x.Note,
                x.Address_nearest,
                x.LoaiPT,
                x.MaPhuongTien,
                NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),
            }).FirstOrDefault();
            return Ok(data);
        }

        [HttpGet("Adsun")]
        public async Task<ActionResult> AdsunAsync(Guid? Id)
        {
            string ToaDo = ""; // Gán giá trị mặc định để tránh lỗi
            // var claims = User.Claims;
            // var maNhanVien = claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            var adsun = new Adsun_DTO();
            HttpClient httpClient = new HttpClient();
            VehicleService vehicleService = new VehicleService(httpClient);
            string[] includes = { "DM_Loai", "DM_Model", "LichSuBaoDuong", "PhuTrachBoPhan" };
            var phuongtien = uow.DS_PhuongTiens.GetSingle(x => !x.IsDeleted && x.Id == Id, includes);
            if (phuongtien != null)
            {
                // var lichsu = uow.MMS_PhuTrachBoPhans.GetAll(x => !x.IsDeleted && x.PhuongTien_Id == phuongtien.Id, x => x.OrderByDescending(x => x.CreatedDate)).FirstOrDefault();
                if (!string.IsNullOrEmpty(phuongtien.BienSo1))
                {
                    VehicleInfo vehicle = await vehicleService.GetVehicleByPlateAsync(phuongtien.BienSo1.Replace("-", "").Replace(".", ""));
                    if (vehicle != null)
                    {
                        //     ToaDo = $"{vehicle.Lat},{vehicle.Lng}";
                        // }
                        adsun.Plate = vehicle.Plate;
                        adsun.ToaDo = $"{vehicle.Lat},{vehicle.Lng}";
                        adsun.Km = vehicle.Km;
                        adsun.Speed = vehicle.Speed;
                        adsun.GroupName = vehicle.GroupName;
                        adsun.Address = vehicle.Address;
                        adsun.Angle = vehicle.Angle;
                        adsun.LoaiPT = phuongtien?.DM_Loai?.Name;
                        adsun.Model = phuongtien?.DM_Model?.Name;
                        adsun.Model_Option = phuongtien?.DM_Model?.Option;
                        adsun.SoKM_Adsun = phuongtien?.SoKM_Adsun.ToString();
                        adsun.TaiXePhuTrach = phuongtien?.PhuTrachBoPhan?.NhanVien;
                        adsun.HinhAnh_TaiXe = phuongtien?.PhuTrachBoPhan?.HinhAnh_NhanVien;
                        adsun.MaNhanVien = phuongtien?.PhuTrachBoPhan?.MaNhanVien;
                        adsun.SoKM = phuongtien?.SoKM.ToString();
                        adsun.SoKMTuNgayBaoDuong = (phuongtien?.SoKM - phuongtien?.SoKM_NgayBaoDuong).ToString();
                        adsun.SoKM_NgayBaoDuong = phuongtien?.SoKM_NgayBaoDuong.ToString();
                        adsun.NgayBaoDuong = string.Format("{0:dd/MM/yyyy HH:mm}", phuongtien?.LichSuBaoDuong?.NgayHoanThanh);
                    }
                }
            }

            return Ok(adsun);
        }







    }
}
