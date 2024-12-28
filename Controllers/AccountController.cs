using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNet.SignalR.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ERP.Helpers;
using ERP.Infrastructure;
using ERP.Models;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using OfficeOpenXml.Style;
using ThacoLibs;
using static ERP.Commons;
using static ERP.Data.MyDbContext;
using static ERP.Helpers.AppSettings;
using Color = System.Drawing.Color;
using System.Dynamic;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Web;
using System.Net.Http;
using static ERP.Helpers.MyTypedClient;
namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly IConfiguration config;
        private readonly DbAdapter dbAdapter;
        private readonly IConfiguration configuration;
        private readonly AppSettings appSettings;
        private readonly MyTypedClient client;
        public AccountController(IConfiguration _configuration, IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, MyTypedClient _client)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            client = _client;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            dbAdapter = new DbAdapter(connectionString);
        }

        //get ramdom code
        private string GenerateRandomSerialNumber()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string serialNumber = new string(Enumerable.Repeat(chars, 32)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            return serialNumber;
        }

        //update hình ảnh
        [HttpPut("hinh-anh/{id}")]
        public async Task<ActionResult> PutHinhAnh(string id)
        {
            try
            {
                var appUser = await userManager.FindByIdAsync(id);
                appUser.HinhAnhUrl = client.AnhNhanVien(appUser.MaNhanVien);
                var result = await userManager.UpdateAsync(appUser);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, $"{appUser.FullName} đã cập nhật hình ảnh thành công");
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Không có hình ảnh hoặc lỗi đã xảy ra!");
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(StatusCodes.Status409Conflict, "Lỗi đã xảy ra!");
            }
        }

        // GET api/account
        [HttpPost]
        public async Task<IActionResult> Post(RegisterModel model)
        {
            string SecurityStampRandom;
            do
            {
                SecurityStampRandom = GenerateRandomSerialNumber();

                // Kiểm tra sự trùng lặp trong CSDL
                var existingItem = userManager.Users.Where(x => x.SecurityStamp == SecurityStampRandom);
                if (existingItem.Count() == 0)
                {
                    break;
                }
            }
            while (true);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (model.Email == null || model.Email == "")
            {
                model.Email = model.MaNhanVien + "@thaco.com.vn";
                var UserName = model.MaNhanVien;
                var passwordhash = Commons.HashPassword(UserName);
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_insertCBNV");
                Guid id = Guid.NewGuid();
                var hinhanh = client.AnhNhanVien(model.MaNhanVien);

                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = UserName;
                dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = false;
                dbAdapter.sqlCommand.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar).Value = SecurityStampRandom;
                dbAdapter.sqlCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordhash;

                var result = dbAdapter.runStoredNoneQuery();
                if (model.chiTiet.Count() > 0)
                {
                    foreach (var item in model.chiTiet)
                    {
                        Guid idct = Guid.NewGuid();
                        dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                        dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                        dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                        dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        dbAdapter.runStoredNoneQuery();
                    }
                }
                dbAdapter.deConnect();
                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                }
                else
                    return Ok("Thêm mới cán bộ nhân viên thành công!");

            }
            else if (model.Email != null && model.Email.Length > 0)
            {
                var passwordhash = Commons.HashPassword(model.MaNhanVien);
                var exit = await userManager.FindByEmailAsync(model.Email);
                // Kiểm tra tài khoản, email có tồn tại không
                //Nếu tài khoản không tồn tại -- Thêm mới
                if (exit == null)
                {
                    dbAdapter.connect();
                    dbAdapter.createStoredProceder("sp_insertCBNV");
                    Guid id = Guid.NewGuid();
                    var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                    dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                    dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                    dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                    dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                    dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                    dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = model.MaNhanVien;
                    dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                    dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                    dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = false;
                    dbAdapter.sqlCommand.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar).Value = SecurityStampRandom;
                    dbAdapter.sqlCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordhash;
                    var result = dbAdapter.runStoredNoneQuery();
                    if (model.chiTiet.Count() > 0)
                    {
                        foreach (var item in model.chiTiet)
                        {
                            Guid idct = Guid.NewGuid();
                            dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                            dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                            dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                            dbAdapter.runStoredNoneQuery();
                        }
                    }
                    dbAdapter.deConnect();
                    if (result == 0)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                    }
                    else
                        return Ok("Thêm mới cán bộ nhân viên thành công!");
                }
                else
                {
                    if (exit.IsDeleted)
                    {
                        exit.UpdatedDate = DateTime.Now;
                        exit.DeletedDate = null;
                        exit.IsDeleted = false;
                        exit.IsActive = false;
                        exit.AccountLevel = false;
                        exit.MustChangePass = false;
                        exit.DonViTraLuong_Id = model.DonViTraLuong_Id;
                        var result = await userManager.UpdateAsync(exit);
                        if (result.Succeeded)
                        {
                            return StatusCode(StatusCodes.Status204NoContent);
                        }
                        return BadRequest(string.Join(",", result.Errors));
                    }
                    return StatusCode(StatusCodes.Status409Conflict, "Thông tin email tài khoản đã tồn tại");
                }
            }
            else
            {
                var exit = await userManager.FindByEmailAsync(model.Email);
                if (exit.IsDeleted)
                {
                    exit.UpdatedDate = DateTime.Now;
                    exit.DeletedDate = null;
                    exit.IsDeleted = false;
                    exit.IsActive = false;
                    exit.AccountLevel = false;
                    exit.MustChangePass = false;
                    exit.DonViTraLuong_Id = model.DonViTraLuong_Id;
                    var result = await userManager.UpdateAsync(exit);
                    if (result.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status204NoContent);
                    }
                    return BadRequest(string.Join(",", result.Errors));
                }
                return StatusCode(StatusCodes.Status409Conflict, "Thông tin email tài khoản đã tồn tại");
            }
        }
        // [HttpGet("Form")]
        // public ActionResult Form(Guid DonVi_Id)
        // {
        //     var query = userManager.Users.Include(x => x.WMS_Kho_PhuTrachBaiXes).Where(x => x.IsActive && !x.IsDeleted).Select(t => new
        //     {
        //         Id = t.Id,
        //         Email = t.Email,
        //         FullName = t.FullName,
        //         t.UserName,
        //         t.GhiChu,
        //         IsUsed = t.WMS_Kho_PhuTrachBaiXes.Count() > 0
        //     }).ToList();
        //     return Ok(query);
        // }

        [HttpPost("post-cbnv")]
        public async Task<IActionResult> PostCBNV(RegisterModel model)
        {
            string SecurityStampRandom;
            do
            {
                SecurityStampRandom = GenerateRandomSerialNumber();

                // Kiểm tra sự trùng lặp trong CSDL
                var existingItem = userManager.Users.Where(x => x.SecurityStamp == SecurityStampRandom);
                if (existingItem.Count() == 0)
                {
                    break;
                }
            }
            while (true);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var exit_MaPin = userManager.Users.FirstOrDefaultAsync(u => u.MaPin == model.MaPin && model.MaPin != null).Result;

            if (exit_MaPin != null)
            {
                return StatusCode(StatusCodes.Status409Conflict, "MaPin đã tồn tại.");
            }
            if (model.Email == null)
            {
                model.Email = model.MaNhanVien + "@thaco.com.vn";
                var UserName = model.MaNhanVien;
                var passwordhash = Commons.HashPassword(UserName);
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_insertCBNV");
                Guid id = Guid.NewGuid();
                var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = UserName;
                dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = false;
                dbAdapter.sqlCommand.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar).Value = SecurityStampRandom;
                dbAdapter.sqlCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordhash;
                dbAdapter.sqlCommand.Parameters.Add("@MaPin", SqlDbType.NVarChar).Value = model.MaPin;
                var result = dbAdapter.runStoredNoneQuery();
                if (model.chiTiet.Count() > 0)
                {
                    foreach (var item in model.chiTiet)
                    {
                        Guid idct = Guid.NewGuid();
                        dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                        dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                        dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                        dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        dbAdapter.runStoredNoneQuery();
                    }
                }
                dbAdapter.deConnect();
                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                }
                else
                    return Ok("Thêm mới cán bộ nhân viên thành công!");
            }
            else
            {
                var passwordhash = Commons.HashPassword(model.MaNhanVien);
                var exit = await userManager.FindByEmailAsync(model.Email);
                // Kiểm tra tài khoản, email có tồn tại không
                //Nếu tài khoản không tồn tại -- Thêm mới
                if (exit == null)
                {
                    dbAdapter.connect();
                    dbAdapter.createStoredProceder("sp_insertCBNV");
                    Guid id = Guid.NewGuid();
                    var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                    dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                    dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                    dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                    dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                    dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                    dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = model.MaNhanVien;
                    dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                    dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                    dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = false;
                    dbAdapter.sqlCommand.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar).Value = SecurityStampRandom;
                    dbAdapter.sqlCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordhash;
                    dbAdapter.sqlCommand.Parameters.Add("@MaPin", SqlDbType.NVarChar).Value = model.MaPin;
                    var result = dbAdapter.runStoredNoneQuery();
                    if (model.chiTiet.Count() > 0 || model != null)
                    {
                        foreach (var item in model.chiTiet)
                        {
                            Guid idct = Guid.NewGuid();
                            dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                            dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                            dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.TapDoan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                            dbAdapter.runStoredNoneQuery();
                        }
                    }
                    dbAdapter.deConnect();
                    if (result == 0)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                    }
                    else
                        return Ok("Thêm mới cán bộ nhân viên thành công!");
                }
                else
                {
                    if (exit.IsDeleted)
                    {
                        exit.UpdatedDate = DateTime.Now;
                        exit.DeletedDate = null;
                        exit.IsDeleted = false;
                        exit.IsActive = false;
                        exit.AccountLevel = false;
                        exit.MustChangePass = false;
                        exit.DonViTraLuong_Id = model.DonViTraLuong_Id;
                        var result = await userManager.UpdateAsync(exit);
                        if (result.Succeeded)
                        {
                            return StatusCode(StatusCodes.Status204NoContent);
                        }
                        return BadRequest(string.Join(",", result.Errors));
                    }
                    return StatusCode(StatusCodes.Status409Conflict, "Thông tin email tài khoản đã tồn tại");
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, UserInfoModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != model.Id)
            {
                return BadRequest();
            }
            int maNhanVien;
            bool isValidInput = false;
            if (Regex.IsMatch(model.MaNhanVien, @"^\d{1,10}$"))
            {
                if (int.TryParse(model.MaNhanVien, out maNhanVien))
                {
                    isValidInput = true;
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Lỗi không xác định xảy ra.");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên chỉ chứa kí tự số!");
            }
            if (model.Email == null)
            {
                model.Email = model.MaNhanVien + "@thaco.com.vn";
                var UserName = model.MaNhanVien;
                var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_updateCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = Guid.Parse(id);
                dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = UserName;
                dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                dbAdapter.sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.NVarChar).Value = DateTime.Now;
                var result = dbAdapter.runStoredNoneQuery();
                var chitiet = uow.chiTiet_DV_PB_BPs.GetAll(x => x.User_Id == Guid.Parse(id)).ToList();
                foreach (var item in chitiet)
                {
                    dbAdapter.createStoredProceder("sp_DeleteChiTiet_DV_PB_BP");
                    dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                    dbAdapter.runStoredNoneQuery();
                }
                if (model.chiTiet.Count() > 0)
                {
                    foreach (var item in model.chiTiet)
                    {
                        Guid idct = Guid.NewGuid();
                        dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                        dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                        dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                        dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        dbAdapter.runStoredNoneQuery();
                    }
                }
                dbAdapter.deConnect();
                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                }
                else
                    return Ok("Chỉnh sửa cán bộ nhân viên thành công!");
            }
            else
            {
                var UserName = model.Email.Split(new[] { '@' })[0];
                var exit = await userManager.FindByEmailAsync(model.Email);
                // Kiểm tra tài khoản, email có tồn tại không
                if (exit != null && exit.Id.ToString() != id)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Thông tin tài khoản, email đã tồn tại");
                }
                var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_updateCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = Guid.Parse(id);
                dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = UserName;
                dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                dbAdapter.sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.NVarChar).Value = DateTime.Now;
                var result = dbAdapter.runStoredNoneQuery();
                var chitiet = uow.chiTiet_DV_PB_BPs.GetAll(x => x.User_Id == Guid.Parse(id)).ToList();
                foreach (var item in chitiet)
                {
                    dbAdapter.createStoredProceder("sp_DeleteChiTiet_DV_PB_BP");
                    dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                    dbAdapter.runStoredNoneQuery();
                }
                if (model.chiTiet.Count() > 0 || model.chiTiet != null)
                {
                    foreach (var item in model.chiTiet)
                    {
                        Guid idct = Guid.NewGuid();
                        dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                        dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                        dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                        dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        dbAdapter.runStoredNoneQuery();
                    }
                }
                dbAdapter.deConnect();
                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                }
                else
                    return Ok("Chỉnh sửa cán bộ nhân viên thành công!");
            }
        }

        [HttpPut("put-active-user/{id}")]
        public async Task<IActionResult> PutActiveUser(string id, UserInfoModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != model.Id)
            {
                return BadRequest();
            }
            var appUser = await userManager.FindByIdAsync(model.Id);
            appUser.IsActive = model.IsActive;
            appUser.UpdatedDate = DateTime.Now;
            var result = await userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                var roles = await userManager.GetRolesAsync(appUser);
                foreach (string item_remove in roles)
                {
                    await userManager.RemoveFromRoleAsync(appUser, item_remove);
                }
                foreach (string RoleName in model.RoleNames)
                {
                    await userManager.AddToRoleAsync(appUser, RoleName);
                }
                return StatusCode(StatusCodes.Status204NoContent);
            }
            else
                return BadRequest(string.Join(",", result.Errors));
        }

        [HttpPut("nghi-viec/{id}")]
        public ActionResult PutNghiViec(string id, NghiViecModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != model.Id)
            {
                return BadRequest();
            }

            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_updateCBNVNghiViec");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = Guid.Parse(id);
            dbAdapter.sqlCommand.Parameters.Add("@NgayNghiViec", SqlDbType.DateTime2).Value = model.NgayNghiViec;
            dbAdapter.sqlCommand.Parameters.Add("@GhiChu", SqlDbType.NVarChar).Value = model.GhiChu;
            dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = false;
            var result = dbAdapter.runStoredNoneQuery();
            if (result == 0)
            {
                model.GhiChuImport = "Nhân viên đang sở hữu thiết bị hoặc đang quản lý kho có thiết bị yêu cầu nhập kho hoặc chuyển người quản lý kho!";
                return StatusCode(StatusCodes.Status409Conflict, model);
            }
            else
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        [HttpPut("put-cbnv/{id}")]
        public async Task<IActionResult> PutCBNV(string id, UserInfoModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != model.Id)
            {
                return BadRequest();
            }

            var exit_MaPin = userManager.Users.FirstOrDefaultAsync(u => u.MaPin == model.MaPin && model.MaPin != null).Result;
            if (exit_MaPin != null)
            {
                return StatusCode(StatusCodes.Status409Conflict, "MaPin đã tồn tại.");
            }
            int maNhanVien;
            bool isValidInput = false;
            if (Regex.IsMatch(model.MaNhanVien, @"^\d{1,10}$"))
            {
                if (int.TryParse(model.MaNhanVien, out maNhanVien))
                {
                    isValidInput = true;
                }
                else
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Lỗi không xác định xảy ra.");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên chỉ chứa kí tự số!");
            }

            if (model.Email == null)
            {
                var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_updateCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = Guid.Parse(id);
                dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = null;
                dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                dbAdapter.sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.NVarChar).Value = DateTime.Now;
                dbAdapter.sqlCommand.Parameters.Add("@MaPin", SqlDbType.NVarChar).Value = model.MaPin;
                var result = dbAdapter.runStoredNoneQuery();
                if (model.chiTiet.Count() > 0)
                {
                    foreach (var item in model.chiTiet)
                    {
                        dbAdapter.createStoredProceder("sp_PutChiTiet_DV_PB_BP");
                        dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = item.Id;
                        dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = item.User_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.TapDoan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@UpdatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        dbAdapter.runStoredNoneQuery();
                    }
                }
                dbAdapter.deConnect();
                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                }
                else
                    return Ok("Chỉnh sửa cán bộ nhân viên thành công!");
            }
            else
            {
                var UserName = model.Email.Split(new[] { '@' })[0];
                var exit = await userManager.FindByEmailAsync(model.Email);
                // Kiểm tra tài khoản, email có tồn tại không
                if (exit != null && exit.Id.ToString() != id)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Thông tin tài khoản, email đã tồn tại");
                }
                var hinhanh = client.AnhNhanVien(model.MaNhanVien);
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_updateCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = Guid.Parse(id);
                dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = model.PhoneNumber;
                dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = model.DonViTraLuong_Id;
                dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = model.MaNhanVien;
                dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = model.FullName;
                dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = model.Email;
                dbAdapter.sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.NVarChar).Value = DateTime.Now;
                dbAdapter.sqlCommand.Parameters.Add("@MaPin", SqlDbType.NVarChar).Value = model.MaPin;
                var result = dbAdapter.runStoredNoneQuery();
                if (model.chiTiet.Count() > 0)
                {
                    foreach (var item in model.chiTiet)
                    {
                        dbAdapter.createStoredProceder("sp_PutChiTiet_DV_PB_BP");
                        dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = item.Id;
                        dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = item.User_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.TapDoan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                        dbAdapter.sqlCommand.Parameters.Add("@UpdatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        dbAdapter.runStoredNoneQuery();
                    }
                }
                dbAdapter.deConnect();
                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Mã nhân viên đã tồn tại.");
                }
                else
                    return Ok("Chỉnh sửa cán bộ nhân viên thành công!");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            var appUser = await userManager.FindByIdAsync(id);
            if (appUser == null)
                return NotFound();
            else
            {
                var role = await userManager.GetRolesAsync(appUser);
                if (role.Count > 0)
                {
                    if (appUser.NghiViec == false)
                    {
                        return Ok(new UserInfoModel
                        {
                            Id = id,
                            Email = appUser.Email,
                            PhoneNumber = appUser.PhoneNumber,
                            UserName = appUser.UserName,
                            MaNhanVien = appUser.MaNhanVien,
                            ChucDanh = appUser.ChucDanh,
                            FullName = appUser.FullName,
                            IsActive = appUser.IsActive,
                            RoleNames = role.ToList(),
                            DonViTraLuong_Id = appUser.DonViTraLuong_Id
                            // DonVi_Id = appUser.DonVi_Id
                        });
                    }
                }
                return BadRequest();
            }
        }

        [HttpGet("cbnv/{id}")]
        public async Task<ActionResult> GetCBNVById(Guid id, Guid? chiTiet_id, Guid? donVi_Id)
        {
            var appUser = await userManager.FindByIdAsync(id.ToString());
            if (appUser == null)
                return NotFound();
            else
            {
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_GetCBNVById");
                dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                dbAdapter.sqlCommand.Parameters.Add("@chiTiet_Id", SqlDbType.UniqueIdentifier).Value = chiTiet_id;
                dbAdapter.sqlCommand.Parameters.Add("@donVi_Id", SqlDbType.UniqueIdentifier).Value = donVi_Id;
                var result = dbAdapter.runStored2Object();
                dbAdapter.deConnect();
                return Ok(result);
            }
        }
        [AllowAnonymous]
        [HttpGet("thongtincbnv")]
        public async Task<ActionResult> Getthongtincbnv(string plainText)
        {
            if (plainText == null)
            {
                return NotFound();
            }
            if (plainText.Length < 10)
            {
                var MaNhanVien = plainText;
                object appUser = client.ThongTinNhanVien(MaNhanVien);
                var taixe = uow.TaiXes.GetSingle(x => x.MaTaiXe == MaNhanVien);
                if (appUser != null)
                {
                    var result = appUser;
                    return Ok(new { result, taixe?.isVaoCong });
                }
                else if (taixe != null)
                {
                    return Ok(new
                    {
                        result = new
                        {
                            maNhanVien = taixe.MaTaiXe,
                            tenNhenVien = taixe.TenTaiXe
                        },
                        isVaoCong = taixe.isVaoCong
                    });
                }
            }
            else
            {
                string modifiedParam = plainText.Contains(" ") ? plainText.Replace(" ", "+") : plainText;
                var MaNhanVien = Commons.DeCode(modifiedParam);
                object appUser = client.ThongTinNhanVien(MaNhanVien);
                var taixe = uow.TaiXes.GetSingle(x => x.MaTaiXe == MaNhanVien);
                if (appUser != null)
                {
                    var result = appUser;
                    return Ok(new { result, taixe?.isVaoCong });
                }
                else if (taixe != null)
                {
                    return Ok(new
                    {
                        result = new
                        {
                            maNhanVien = taixe.MaTaiXe,
                            tenNhenVien = taixe.TenTaiXe
                        },
                        isVaoCong = taixe.isVaoCong
                    });
                }
            }


            return NotFound();

        }


        [HttpGet("Get-NonActive-User")]
        public ActionResult GetNonActiveUser(string keyword = null, Guid? donviId = null, Guid? phanMemId = null)
        {
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetListUserCBNV_NonActive");
            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
            dbAdapter.sqlCommand.Parameters.Add("@phanMem_Id", SqlDbType.UniqueIdentifier).Value = phanMemId;
            var list = dbAdapter.runStored2ObjectList();
            dbAdapter.deConnect();
            return Ok(list);
        }
        [HttpGet("GetListUser")]
        public ActionResult GetListUser(string keyword = null, Guid? donviId = null)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
            }
            var query = userManager.Users.Where(x => (string.IsNullOrEmpty(keyword)
            || x.Email.ToLower().Contains(keyword.ToLower())
            || x.UserName.ToLower().Contains(keyword.ToLower())
            || x.FullName.ToLower().Contains(keyword.ToLower())
            || x.MaNhanVien.ToLower().Contains(keyword.ToLower())) && !x.IsDeleted);
            List<ListUserModel> list = new List<ListUserModel>();

            foreach (var item in query)
            {
                var infor = new ListUserModel();
                if (item.NghiViec == false && item.IsActive == false)
                {
                    var donvi = uow.DonVis.GetAll(x => x.Id == item.DonViTraLuong_Id).SingleOrDefault();
                    infor.Id = item.Id.ToString();
                    infor.FullName = item.FullName;
                    infor.ChucDanh = item.ChucDanh;
                    infor.DonViTraLuong_Id = item.DonViTraLuong_Id;
                    if (donvi != null)
                    {
                        infor.TenDonViTraLuong = donvi.TenDonVi;
                    }
                    else
                    {
                        infor.TenDonViTraLuong = string.Empty; // hoặc giá trị mặc định khác tùy thuộc vào yêu cầu của bạn
                    }
                    list.Add(infor);
                }
            }
            return Ok(list);
        }

        [HttpGet("Get-List-User-nghi-viec")]
        public ActionResult GetListUserNghiViec(string keyword = null, Guid? donviId = null)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
            }
            var query = userManager.Users.Where(x => (string.IsNullOrEmpty(keyword)
            || x.Email.ToLower().Contains(keyword.ToLower())
            || x.UserName.ToLower().Contains(keyword.ToLower())
            || x.FullName.ToLower().Contains(keyword.ToLower())) && !x.IsDeleted);
            List<ListUserModelNghiViec> list = new List<ListUserModelNghiViec>();

            foreach (var item in query)
            {
                var infor = new ListUserModelNghiViec();
                if (item.NghiViec == true)
                {
                    var donvi = uow.DonVis.GetAll(x => x.Id == item.DonViTraLuong_Id).SingleOrDefault();
                    infor.Id = item.Id.ToString();
                    infor.FullName = item.FullName;
                    infor.ChucDanh = item.ChucDanh;
                    infor.DonViTraLuong_Id = item.DonViTraLuong_Id;
                    infor.MaNhanVien = item.MaNhanVien;
                    infor.PhoneNumber = item.PhoneNumber;
                    infor.Email = item.Email;
                    if (donvi != null)
                    {
                        infor.TenDonViTraLuong = donvi.TenDonVi;
                    }
                    else
                    {
                        infor.TenDonViTraLuong = string.Empty; // hoặc giá trị mặc định khác tùy thuộc vào yêu cầu của bạn
                    }
                    string ngayNghiViecFormatted = item.NgayNghiViec.ToString();
                    infor.NgayNghiViec = DateTime.Parse(ngayNghiViecFormatted).ToString("dd/MM/yyyy");
                    infor.GhiChu = item.GhiChu;
                    list.Add(infor);
                }
            }
            return Ok(list);
        }

        [HttpGet]
        public ActionResult Get(int page = 1, string keyword = null, Guid? donviId = null)
        {
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetListUser");
            dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
            var result = dbAdapter.runStored2ObjectList();
            dbAdapter.deConnect();
            int totalRow = result.Count();
            int pageSize = pageSizeData.PageSize;
            int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);
            if (page < 1)
            {
                page = 1;
            }
            else if (page > totalPage)
            {
                page = totalPage;
            }

            var datalist = result.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new
            {
                totalRow,
                totalPage,
                pageSize,
                datalist
            });
        }

        [HttpGet("get-cbnv")]
        public ActionResult GetCBNV(int key = 0, int page = 1, string keyword = null, Guid? donviId = null, Guid? phongbanId = null, Guid? boPhanId = null)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
            }

            if (key == 1)
            {

                var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_GetListCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
                dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
                dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = phongbanId;
                dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = boPhanId;
                dbAdapter.sqlCommand.Parameters.Add("@key", SqlDbType.Int).Value = key;
                var result = dbAdapter.runStored2ObjectList();
                dbAdapter.deConnect();
                return Ok(result);
            }
            else
            {
                var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_GetListCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
                dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
                dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = phongbanId;
                dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = boPhanId;
                dbAdapter.sqlCommand.Parameters.Add("@key", SqlDbType.Int).Value = key;
                var result = dbAdapter.runStored2ObjectList();
                dbAdapter.deConnect();
                List<object> GetDataFunction = result;
                List<System.Dynamic.ExpandoObject> expandoList = result.OfType<System.Dynamic.ExpandoObject>().ToList();
                if (expandoList.Count > 0)
                {
                    int totalRow = result.Count();
                    int pageSize = pageSizeData.PageSize;
                    int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);

                    if (page < 1)
                    {
                        page = 1;
                    }
                    else if (page > totalPage)
                    {
                        page = totalPage;
                    }

                    var datalist = result.Skip((page - 1) * pageSize).Take(pageSize);
                    return Ok(new
                    {
                        totalRow,
                        totalPage,
                        pageSize,
                        datalist
                    });
                }
                else
                {
                    if (keyword != null && donviId == null)
                    {
                        dbAdapter.connect();
                        dbAdapter.createStoredProceder("sp_GetCBNVByMaNhanVien");
                        dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = keyword;
                        var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                        var roles = userManager.GetRolesAsync(exit).Result;
                        var isAdmin = roles.Contains("Administrator");
                        if (isAdmin)
                        {
                            dbAdapter.sqlCommand.Parameters.Add("@numberkey", SqlDbType.Int).Value = 1;
                        }
                        else
                            dbAdapter.sqlCommand.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        var results = dbAdapter.runStored2ObjectList();
                        dbAdapter.deConnect();
                        int totalRow = results.Count();
                        int pageSize = pageSizeData.PageSize;
                        int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);
                        if (page < 1)
                        {
                            page = 1;
                        }
                        else if (page > totalPage)
                        {
                            page = totalPage;
                        }

                        var datalist = results.Skip((page - 1) * pageSize).Take(pageSize);
                        return Ok(new
                        {
                            totalRow,
                            totalPage,
                            pageSize,
                            datalist
                        });
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
        }
        [HttpGet("get-cbnv-phanquyen")]
        public ActionResult GetCBNVPhanQuyen(int key = 0, int page = 1, string keyword = null, Guid? donviId = null, Guid? phongbanId = null, Guid? boPhanId = null)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Trim();
            }

            if (key == 1)
            {

                var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_GetListCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
                dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
                dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = phongbanId;
                dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = boPhanId;
                dbAdapter.sqlCommand.Parameters.Add("@key", SqlDbType.Int).Value = key;
                var result = dbAdapter.runStored2ObjectList();
                dbAdapter.deConnect();
                return Ok(result);
            }
            else
            {
                var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
                dbAdapter.connect();
                dbAdapter.createStoredProceder("sp_GetListCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
                dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
                dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = phongbanId;
                dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = boPhanId;
                dbAdapter.sqlCommand.Parameters.Add("@key", SqlDbType.Int).Value = key;
                var result = dbAdapter.runStored2ObjectList();
                dbAdapter.deConnect();
                List<object> GetDataFunction = result;
                List<System.Dynamic.ExpandoObject> expandoList = result.OfType<System.Dynamic.ExpandoObject>().ToList();
                if (expandoList.Count > 0)
                {

                    return Ok(result);
                }
                else
                {
                    if (keyword != null && donviId == null)
                    {
                        dbAdapter.connect();
                        dbAdapter.createStoredProceder("sp_GetCBNVByMaNhanVien");
                        dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = keyword;
                        var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                        var roles = userManager.GetRolesAsync(exit).Result;
                        var isAdmin = roles.Contains("Administrator");
                        if (isAdmin)
                        {
                            dbAdapter.sqlCommand.Parameters.Add("@numberkey", SqlDbType.Int).Value = 1;
                        }
                        else
                            dbAdapter.sqlCommand.Parameters.Add("@userId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                        var results = dbAdapter.runStored2ObjectList();
                        dbAdapter.deConnect();
                        return Ok(results);
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
        }
        [HttpGet("get-cbnv-add-role")]
        public ActionResult GetCBNVAddRole(int key = 0, int page = 1, string keyword = null)
        {
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetListCBNVAddRole");
            dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
            var result = dbAdapter.runStored2ObjectList();
            dbAdapter.deConnect();
            return Ok(result);
        }
        [HttpGet("list-user-cbnv-role")]
        public ActionResult GetListUserCBNVRole(int page = 1, string keyword = null, Guid? donviId = null, Guid? phanMem_Id = null)
        {
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetListUserCBNV_Role");
            dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
            dbAdapter.sqlCommand.Parameters.Add("@phanMem_Id", SqlDbType.UniqueIdentifier).Value = phanMem_Id;
            var result = dbAdapter.runStored2ObjectList();
            dbAdapter.deConnect();
            int totalRow = result.Count();
            int pageSize = pageSizeData.PageSize;
            int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);
            if (page < 1)
            {
                page = 1;
            }
            else if (page > totalPage)
            {
                page = totalPage;
            }

            var datalist = result.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new
            {
                totalRow,
                totalPage,
                pageSize,
                datalist
            });
        }
        [HttpPost("user-cbnv")]
        public ActionResult PostUserCBNV(Guid id, ClassCBNVActive model)
        {
            dbAdapter.connect();
            foreach (var item in model.chiTietRoles)
            {
                dbAdapter.createStoredProceder("sp_GetRoleCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                dbAdapter.sqlCommand.Parameters.Add("@phanMem_Id", SqlDbType.UniqueIdentifier).Value = model.PhanMem_Id;
                dbAdapter.sqlCommand.Parameters.Add("@donVi_Id", SqlDbType.UniqueIdentifier).Value = model.DonVi_Id;
                var result = dbAdapter.runStored2Object();
                if (result is ExpandoObject expandoObj && expandoObj.Count() <= 0)
                {
                    dbAdapter.deConnect();
                    return StatusCode(StatusCodes.Status409Conflict, "Vai trò phải thuộc phần mềm và đơn vị sở hữu!");
                }
            }
            foreach (var item in model.chiTietRoles)
            {
                dbAdapter.createStoredProceder("sp_GetCheckExistsRole");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                var result = dbAdapter.runStored2Object();
                if (result is ExpandoObject expandoObj && expandoObj.Count() > 0)
                {
                    dbAdapter.deConnect();
                    return StatusCode(StatusCodes.Status409Conflict, "Vai trò này đã có ở người dùng này!");
                }
            }
            foreach (var item in model.chiTietRoles)
            {
                dbAdapter.createStoredProceder("sp_PostUserCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                dbAdapter.sqlCommand.Parameters.Add("@phanMem_Id", SqlDbType.UniqueIdentifier).Value = model.PhanMem_Id;
                dbAdapter.sqlCommand.Parameters.Add("@donVi_Id", SqlDbType.UniqueIdentifier).Value = model.DonVi_Id;
                var result = dbAdapter.runStoredNoneQuery();
            }
            dbAdapter.deConnect();
            return Ok("Thêm mới vai trò thành công!");
        }

        [HttpPut("user-cbnv")]
        public ActionResult PutUserCBNV(ClassCBNVActive model)
        {
            dbAdapter.connect();

            foreach (var item in model.chiTietRoles)
            {
                dbAdapter.createStoredProceder("sp_GetRoleCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                dbAdapter.sqlCommand.Parameters.Add("@phanMem_Id", SqlDbType.UniqueIdentifier).Value = model.PhanMem_Id;
                dbAdapter.sqlCommand.Parameters.Add("@donVi_Id", SqlDbType.UniqueIdentifier).Value = model.DonVi_Id;
                var result = dbAdapter.runStored2Object();
                if (result is ExpandoObject expandoObj && expandoObj.Count() <= 0)
                {
                    dbAdapter.deConnect();
                    return StatusCode(StatusCodes.Status409Conflict, "Vai trò phải thuộc phần mềm và đơn vị sở hữu!");
                }
            }

            foreach (var item in model.chiTietRolesOld)
            {
                dbAdapter.createStoredProceder("sp_DeleteRoleAdmin");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Roleold_Id;
                dbAdapter.runStoredNoneQuery();
            }

            foreach (var item in model.chiTietRoles)
            {
                dbAdapter.createStoredProceder("sp_PostUserCBNV");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
                dbAdapter.sqlCommand.Parameters.Add("@phanMem_Id", SqlDbType.UniqueIdentifier).Value = model.PhanMem_Id;
                dbAdapter.sqlCommand.Parameters.Add("@donVi_Id", SqlDbType.UniqueIdentifier).Value = model.DonVi_Id;
                var result = dbAdapter.runStoredNoneQuery();
            }

            dbAdapter.deConnect();
            return Ok("Chỉnh sửa vai trò thành công!");
        }

        [HttpDelete("user-cbnv")]
        public ActionResult DeleteUserCBNV(Guid id, Guid role_Id)
        {
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_DeleteRoleAdmin");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = role_Id;
            dbAdapter.runStoredNoneQuery();
            dbAdapter.deConnect();
            return Ok("Xóa vai trò thành công!");
        }

        [HttpPut("Active/{id}")]
        public async Task<ActionResult> Active(string id)
        {
            var appUser = await userManager.FindByIdAsync(id);
            appUser.IsActive = !appUser.IsActive;
            appUser.UpdatedDate = DateTime.Now;
            var result = await userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                if (appUser.IsActive)
                {
                    return StatusCode(StatusCodes.Status200OK, "Mở khóa tài khoản thành công");
                }
                return StatusCode(StatusCodes.Status200OK, "Khóa tài khoản thành công");
            }
            return BadRequest(string.Join(",", result.Errors));
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_DeleteCBNV");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
            dbAdapter.sqlCommand.Parameters.Add("@IsDeleted", SqlDbType.Bit).Value = true;
            dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = false;

            var result = dbAdapter.runStoredNoneQuery();
            dbAdapter.deConnect();
            return Ok(result);
        }

        [HttpPut("ResetPassword/{id}")]
        public async Task<ActionResult> ResetPassword(string id)
        {
            var appUser = await userManager.FindByIdAsync(id);
            var Password = HashPassword(appUser.MaNhanVien);
            appUser.UpdatedDate = DateTime.Now;
            appUser.MustChangePass = false;
            appUser.PasswordHash = Password;
            var result = await userManager.UpdateAsync(appUser);
            if (result.Succeeded)
            {
                return StatusCode(StatusCodes.Status200OK, "Khôi phục mật khẩu mặc định thành công");
            }
            return BadRequest(string.Join(",", result.Errors));
        }

        [HttpPost("ChangePassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var appUser = await userManager.FindByIdAsync(User.Identity.Name);
            appUser.MustChangePass = false;
            appUser.UpdatedDate = DateTime.Now;
            var checklogin = VerifyPassword(model.Password, appUser.PasswordHash);
            var passwordhash = HashPassword(model.NewPassword);
            if (checklogin)
            {
                if (model.NewPassword == model.ConfirmNewPassword)
                {
                    appUser.PasswordHash = passwordhash;
                    var result = await userManager.UpdateAsync(appUser);
                    if (result.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status200OK, "Đổi mật khẩu thành công");
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Lỗi không xác định");
                    }
                }
                else
                {
                    return BadRequest("Mật khẩu mới và mật khẩu xác định không trùng khớp");
                }

            }
            else
            {
                return BadRequest("Mật khẩu hiện tại không đúng");
            }

        }

        [HttpGet("get-user-by-quanly")]
        public IActionResult GetUserByQuanLy()
        {
            dbAdapter.connect(); //Mở kết nối
            dbAdapter.createStoredProceder("sp_Getuserbyquanly"); //truyền tên thủ tục cần gọi
            dbAdapter.sqlCommand.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
            var result = dbAdapter.runStored2JSON(); //gọi method xử lý trả về kết quả trong biến result

            dbAdapter.deConnect(); //Chú ý nhớ phải gọi hàm đóng kết nối
            return Ok(result);
        }

        [HttpGet("get-userroleit-by-quanly")]
        public IActionResult GetUserRoleItByQL()
        {
            dbAdapter.connect(); //Mở kết nối
            dbAdapter.createStoredProceder("sp_GetUserRolesITByQuanLy"); //truyền tên thủ tục cần gọi
            var result = dbAdapter.runStored2JSON(); //gọi method xử lý trả về kết quả trong biến result
            dbAdapter.deConnect(); //Chú ý nhớ phải gọi hàm đóng kết nối
            return Ok(result);
        }

        [HttpGet("get-user-role")]
        public IActionResult GetUserRole(Guid id)
        {
            dbAdapter.connect(); //Mở kết nối
            dbAdapter.createStoredProceder("sp_GetRoleNguoiDungById"); //truyền tên thủ tục cần gọi
            dbAdapter.sqlCommand.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = id;
            var result = dbAdapter.runStored2JSON(); //gọi method xử lý trả về kết quả trong biến result

            dbAdapter.deConnect(); //Chú ý nhớ phải gọi hàm đóng kết nối
            return Ok(result);
        }

        public class DropdownTreeNode
        {
            public Guid Id { get; set; }
            public string NameId { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public List<DropdownTreeNode> Children { get; set; }
            public bool? Disable { get; set; }
        }

        [HttpGet("dropdown")]
        public IActionResult GetDropdownData(Guid? donviId = null, Guid? phongbanId = null, Guid? bophanId = null)
        {
            var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
            var roles = userManager.GetRolesAsync(exit).Result;
            var isAdmin = roles.Contains("Administrator");
            var tapdoans = uow.tapDoans.GetAll(x => !x.IsDeleted).ToList();
            var result = new List<DropdownTreeNode>(); // Danh sách lưu trữ các đối tượng

            foreach (var tapdoan in tapdoans)
            {
                var donviRoot = new DropdownTreeNode
                {
                    Id = tapdoan.Id,
                    Name = tapdoan.TenTapDoan,
                    NameId = tapdoan.Id.ToString(),
                    Level = 0,
                    Children = new List<DropdownTreeNode>()
                };

                if (!donviId.HasValue && !phongbanId.HasValue && !bophanId.HasValue)
                {
                    // Lấy danh sách đơn vị thuộc tập đoàn
                    var donvis = uow.DonVis.GetAll(x => x.TapDoan_Id == tapdoan.Id && !x.IsDeleted);
                    foreach (var donvi in donvis)
                    {
                        var donviNode = new DropdownTreeNode
                        {
                            Id = donvi.Id,
                            NameId = tapdoan.Id + "_" + donvi.Id,
                            Name = donvi.TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode>()
                        };

                        // Lấy danh sách phòng ban thuộc đơn vị
                        var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi.Id && !x.IsDeleted);
                        foreach (var phongban in phongbans)
                        {
                            var phongbanNode = new DropdownTreeNode
                            {
                                Id = phongban.Id,
                                NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id,
                                Name = phongban.TenPhongBan,
                                Level = 2,
                                Children = new List<DropdownTreeNode>(),
                                Disable = false
                            };

                            // Lấy danh sách bộ phận thuộc phòng ban
                            var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == phongban.Id && !x.IsDeleted);
                            foreach (var bophan in bophans)
                            {
                                var bophanNode = new DropdownTreeNode
                                {
                                    Id = bophan.Id,
                                    NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id + "_" + bophan.Id,
                                    Name = bophan.TenBoPhan,
                                    Level = 3,
                                    Children = new List<DropdownTreeNode>(),
                                    Disable = true
                                };

                                phongbanNode.Children.Add(bophanNode);
                            }
                            if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                            {
                                phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                            }

                            donviNode.Children.Add(phongbanNode);
                        }

                        donviRoot.Children.Add(donviNode);
                    }

                    result.Add(donviRoot);
                }

                if (donviId.HasValue && !phongbanId.HasValue && !bophanId.HasValue)
                {
                    var donvi = uow.DonVis.GetAll(x => x.Id == donviId).ToList();
                    if (donvi != null)
                    {
                        var donviNode = new DropdownTreeNode
                        {
                            Id = donvi[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id,
                            Name = donvi[0].TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode>()
                        };

                        // Lấy danh sách phòng ban thuộc đơn vị
                        var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi[0].Id && !x.IsDeleted);
                        foreach (var phongban in phongbans)
                        {
                            var phongbanNode = new DropdownTreeNode
                            {
                                Id = phongban.Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban.Id,
                                Name = phongban.TenPhongBan,
                                Level = 2,
                                Children = new List<DropdownTreeNode>(),
                                Disable = false
                            };

                            // Lấy danh sách bộ phận thuộc phòng ban
                            var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == phongban.Id && !x.IsDeleted);
                            foreach (var bophan in bophans)
                            {
                                var bophanNode = new DropdownTreeNode
                                {
                                    Id = bophan.Id,
                                    NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban.Id + "_" + bophan.Id,
                                    Name = bophan.TenBoPhan,
                                    Level = 3,
                                    Children = new List<DropdownTreeNode>(),
                                    Disable = true
                                };

                                phongbanNode.Children.Add(bophanNode);
                            }
                            if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                            {
                                phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                            }
                            donviNode.Children.Add(phongbanNode);
                        }

                        donviRoot.Children.Add(donviNode);
                    }

                    result.Add(donviRoot);
                }

                if (donviId.HasValue && phongbanId.HasValue && !bophanId.HasValue)
                {
                    var donvi = uow.DonVis.GetAll(x => x.Id == donviId).ToList();
                    var phongban = uow.phongbans.GetAll(x => x.Id == phongbanId).ToList();
                    if (donvi != null && phongban != null && phongban[0].DonVi_Id == donvi[0].Id)
                    {
                        var donviNode = new DropdownTreeNode
                        {
                            Id = donvi[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id,
                            Name = donvi[0].TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode>()
                        };

                        var phongbanNode = new DropdownTreeNode
                        {
                            Id = phongban[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id,
                            Name = phongban[0].TenPhongBan,
                            Level = 2,
                            Children = new List<DropdownTreeNode>(),
                            Disable = false
                        };

                        // Lấy danh sách bộ phận thuộc phòng ban
                        var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == phongban[0].Id && !x.IsDeleted);
                        foreach (var bophan in bophans)
                        {
                            var bophanNode = new DropdownTreeNode
                            {
                                Id = bophan.Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id + "_" + bophan.Id,
                                Name = bophan.TenBoPhan,
                                Level = 3,
                                Children = new List<DropdownTreeNode>(),
                                Disable = true
                            };

                            phongbanNode.Children.Add(bophanNode);
                        }
                        if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                        {
                            phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                        }

                        donviNode.Children.Add(phongbanNode);
                        donviRoot.Children.Add(donviNode);
                    }

                    result.Add(donviRoot);
                }

                if (donviId.HasValue && phongbanId.HasValue && bophanId.HasValue)
                {
                    var donvi = uow.DonVis.GetAll(x => x.Id == donviId).ToList();
                    var phongban = uow.phongbans.GetAll(x => x.Id == phongbanId).ToList();
                    var bophan = uow.BoPhans.GetAll(x => x.Id == bophanId).ToList();
                    if (donvi != null && phongban != null && bophan != null && phongban[0].DonVi_Id == donvi[0].Id && bophan[0].PhongBan_Id == phongban[0].Id)
                    {
                        var donviNode = new DropdownTreeNode
                        {
                            Id = donvi[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id,
                            Name = donvi[0].TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode>()
                        };

                        var phongbanNode = new DropdownTreeNode
                        {
                            Id = phongban[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id,
                            Name = phongban[0].TenPhongBan,
                            Level = 2,
                            Children = new List<DropdownTreeNode>(),
                            Disable = false
                        };

                        var bophanNode = new DropdownTreeNode
                        {
                            Id = bophan[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id + "_" + bophan[0].Id,
                            Name = bophan[0].TenBoPhan,
                            Level = 3,
                            Children = new List<DropdownTreeNode>(),
                            Disable = true
                        };

                        phongbanNode.Children.Add(bophanNode);
                        if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                        {
                            phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                        }
                        donviNode.Children.Add(phongbanNode);
                        donviRoot.Children.Add(donviNode);
                    }

                    result.Add(donviRoot);
                }
            }

            return Ok(result);
        }

        public class ClassCBNVImport
        {
            [Display(Name = "Email")]
            public string Email { get; set; }
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }
            [RegularExpression(@"^\d{1,10}$", ErrorMessage = "Mã nhân viên chỉ được chứa số và có độ dài nhỏ hơn hoặc bằng 10 chữ số.")]
            [Required]
            public string MaNhanVien { get; set; }
            public string FullName { get; set; }
            public Guid? DonVi_Id { get; set; }
            public Guid? BoPhan_Id { get; set; }
            public Guid? ChucVu_Id { get; set; }
            public Guid? PhongBan_Id { get; set; }
            public Guid? DonViTraLuong_Id { get; set; }
            public Guid? TapDoan_Id { get; set; }
            public string MaTapDoan { get; set; }
            public string MaDonVi { get; set; }
            public string MaDonViTraLuong { get; set; }
            public string MaBoPhan { get; set; }
            public string MaChucVu { get; set; }
            public string MaPhongBan { get; set; }
            public string ClassName { get; set; }
            public string GhiChuImport { get; set; }
        }

        [HttpPost("ImportExel")]
        public async Task<IActionResult> ImportExel(List<ClassCBNVImport> data)
        {
            var donvi = uow.DonVis.GetAll(x => !x.IsDeleted);
            var phongban = uow.phongbans.GetAll(x => !x.IsDeleted);
            var bophan = uow.BoPhans.GetAll(x => !x.IsDeleted);
            var chucvu = uow.chucVus.GetAll(x => !x.IsDeleted);
            var tapdoan = uow.tapDoans.GetAll(x => !x.IsDeleted);
            var successItems = new List<ClassCBNVImport>();
            var chitietitem = new List<ChiTiet_DV_PB_BP>();
            foreach (var item in data)
            {
                item.ClassName = "new";
                var td = tapdoan.SingleOrDefault(x => x.MaTapDoan.ToLower() == item.MaTapDoan.ToLower());
                var dv = donvi.SingleOrDefault(x => x.MaDonVi.ToLower() == item.MaDonVi.ToLower() && td.Id == x.TapDoan_Id);
                var dvtl = donvi.SingleOrDefault(x => x.MaDonVi.ToLower() == item.MaDonViTraLuong.ToLower() && td.Id == x.TapDoan_Id);
                var pb = phongban.SingleOrDefault(x => x.MaPhongBan.ToLower() == item.MaPhongBan.ToLower() && dv.Id == x.DonVi_Id);
                var cv = chucvu.SingleOrDefault(x => x.MaChucVu.ToLower() == item.MaChucVu.ToLower());
                var bp = bophan.SingleOrDefault(x => x?.MaBoPhan?.ToLower() == item.MaBoPhan?.ToLower() && pb?.Id == x?.PhongBan_Id);
                var nv = userManager.Users.AsNoTracking().FirstOrDefault(x => x.MaNhanVien == item.MaNhanVien && !x.IsDeleted);
                if (nv != null)
                {
                    item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " đã tồn tại!";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                try
                {
                    item.DonVi_Id = dv.Id;
                    if (bp != null)
                    {
                        item.BoPhan_Id = bp.Id;
                    }
                    item.PhongBan_Id = pb.Id;
                    item.DonViTraLuong_Id = dvtl.Id;
                    item.ChucVu_Id = cv.Id;
                    item.TapDoan_Id = td.Id;
                }
                catch
                {
                    if (td == null)
                    {
                        item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có mã tập đoàn không đúng!";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else if (dv == null)
                    {
                        item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có mã đơn vị không đúng!";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else if (dvtl == null)
                    {
                        item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có mã đơn vị trả lương không đúng!";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else if (cv == null)
                    {
                        item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có mã chức vụ không đúng!";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else if (pb == null)
                    {
                        item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có mã phòng ban không đúng hoặc không thuộc đơn vị!";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                }
                if (bp == null && item.MaBoPhan != null)
                {
                    item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có mã bộ phận không đúng!";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                if (item.Email != null)
                {
                    var UserName = item.Email.Split(new[] { '@' })[0];
                    var exit_username = await userManager.FindByNameAsync(UserName);
                    var exit_email = await userManager.FindByEmailAsync(item.Email);
                    if (exit_username == null && exit_email == null)
                    {
                        var user = new ApplicationUser()
                        {
                            MaNhanVien = item.MaNhanVien,
                            PhoneNumber = item.PhoneNumber,
                            DonViTraLuong_Id = item.DonViTraLuong_Id,
                            UserName = UserName,
                            FullName = item.FullName,
                            Email = item.Email,
                            IsActive = false,
                            MustChangePass = false,
                            CreatedDate = DateTime.Now
                        };
                        successItems.Add(item);
                        if (bp != null)
                        {
                            var c = new ChiTiet_DV_PB_BP()
                            {
                                TapDoan_Id = td.Id,
                                DonVi_Id = dv.Id,
                                PhongBan_Id = pb.Id,
                                BoPhan_Id = bp.Id,
                                ChucVu_Id = item.ChucVu_Id,
                            };
                            chitietitem.Add(c);
                        }
                        else
                        {
                            var c = new ChiTiet_DV_PB_BP()
                            {
                                TapDoan_Id = td.Id,
                                DonVi_Id = dv.Id,
                                PhongBan_Id = pb.Id,
                                ChucVu_Id = item.ChucVu_Id,
                            };
                            chitietitem.Add(c);
                        }
                    }
                    else
                    {
                        item.GhiChuImport = "Mã nhân viên " + item.MaNhanVien + " có email tài khoản đã tồn tại!";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                }
                else
                {
                    var UserName = item.Email;
                    var user = new ApplicationUser()
                    {
                        MaNhanVien = item.MaNhanVien,
                        PhoneNumber = item.PhoneNumber,
                        DonViTraLuong_Id = item.DonViTraLuong_Id,
                        UserName = UserName,
                        FullName = item.FullName,
                        Email = item.Email,
                        IsActive = false,
                        MustChangePass = false,
                        CreatedDate = DateTime.Now
                    };
                    successItems.Add(item);
                    if (bp != null)
                    {
                        var c = new ChiTiet_DV_PB_BP()
                        {
                            TapDoan_Id = td.Id,
                            DonVi_Id = dv.Id,
                            PhongBan_Id = pb.Id,
                            BoPhan_Id = bp.Id,
                            ChucVu_Id = item.ChucVu_Id,
                        };
                        chitietitem.Add(c);
                    }
                    else
                    {
                        var c = new ChiTiet_DV_PB_BP()
                        {
                            TapDoan_Id = td.Id,
                            DonVi_Id = dv.Id,
                            PhongBan_Id = pb.Id,
                            ChucVu_Id = item.ChucVu_Id,
                        };
                        chitietitem.Add(c);
                    }
                }

            }
            try
            {
                List<ApplicationUser> usersToInsert = new List<ApplicationUser>();
                List<ChiTiet_DV_PB_BP> chitiet = new List<ChiTiet_DV_PB_BP>();
                foreach (var item in successItems)
                {
                    foreach (var ct in chitietitem)
                    {
                        if (item.Email == null)
                        {
                            var user = new ApplicationUser()
                            {
                                MaNhanVien = item.MaNhanVien,
                                PhoneNumber = item.PhoneNumber,
                                DonViTraLuong_Id = item.DonViTraLuong_Id,
                                FullName = item.FullName,
                                Email = item.Email,
                                IsActive = false,
                                MustChangePass = false,
                                CreatedDate = DateTime.Now,
                                LockoutEnabled = true,
                            };
                            usersToInsert.Add(user);
                            var c = new ChiTiet_DV_PB_BP()
                            {
                                TapDoan_Id = ct.TapDoan_Id,
                                DonVi_Id = ct.DonVi_Id,
                                PhongBan_Id = ct.PhongBan_Id,
                                BoPhan_Id = ct.BoPhan_Id,
                                ChucVu_Id = ct.ChucVu_Id,
                            };
                            chitiet.Add(c);
                        }
                        else
                        {
                            var UserName = item.Email.Split(new[] { '@' })[0];
                            var user = new ApplicationUser()
                            {
                                MaNhanVien = item.MaNhanVien,
                                PhoneNumber = item.PhoneNumber,
                                DonViTraLuong_Id = item.DonViTraLuong_Id,
                                UserName = UserName,
                                FullName = item.FullName,
                                Email = item.Email,
                                IsActive = false,
                                MustChangePass = false,
                                CreatedDate = DateTime.Now,
                                LockoutEnabled = true,
                            };
                            usersToInsert.Add(user);
                            var c = new ChiTiet_DV_PB_BP()
                            {
                                TapDoan_Id = ct.TapDoan_Id,
                                DonVi_Id = ct.DonVi_Id,
                                PhongBan_Id = ct.PhongBan_Id,
                                BoPhan_Id = ct.BoPhan_Id,
                                ChucVu_Id = ct.ChucVu_Id,
                            };
                            chitiet.Add(c);
                        }
                    }

                }

                // Perform the database insertions in a transaction
                List<string> list = new List<string>();

                foreach (var user in usersToInsert)
                {
                    foreach (var item in chitiet)
                    {
                        var query = userManager.Users.Where(x => x.MaNhanVien == user.MaNhanVien && x.IsDeleted).SingleOrDefault();
                        if (query != null)
                        {
                            if (user.Email == null || user.Email == "")
                            {
                                user.Email = user.MaNhanVien + "@thaco.com.vn";
                            }
                            var hinhanh = client.AnhNhanVien(user.MaNhanVien);
                            dbAdapter.connect();
                            dbAdapter.createStoredProceder("sp_updateCBNV");
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = query.Id;
                            dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = user.MaNhanVien;
                            dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                            dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = user.PhoneNumber;
                            dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = user.DonViTraLuong_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = user.MaNhanVien;
                            dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = user.FullName;
                            dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = user.Email;
                            dbAdapter.sqlCommand.Parameters.Add("@CreatedDate", SqlDbType.NVarChar).Value = DateTime.Now;
                            dbAdapter.runStoredNoneQuery();

                            dbAdapter.createStoredProceder("sp_PutChiTiet_DV_PB_BP");
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = item.Id;
                            dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = query.Id;
                            dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.TapDoan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@UpdatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                            dbAdapter.runStoredNoneQuery();

                        }
                        else
                        {
                            string SecurityStampRandom;
                            do
                            {
                                SecurityStampRandom = GenerateRandomSerialNumber();
                                var existingItem = userManager.Users.Where(x => x.SecurityStamp == SecurityStampRandom);

                                if (existingItem.Count() == 0)
                                {
                                    if (!list.Contains(SecurityStampRandom))
                                    {
                                        list.Add(SecurityStampRandom);
                                        break;
                                    }
                                }
                            }
                            while (true);
                            var passwordhash = Commons.HashPassword(user.MaNhanVien);
                            if (user.Email == null || user.Email == "")
                            {
                                user.Email = user.MaNhanVien + "@thaco.com.vn";
                            }
                            dbAdapter.connect();
                            dbAdapter.createStoredProceder("sp_insertCBNV");
                            Guid id = Guid.NewGuid();
                            var hinhanh = client.AnhNhanVien(user.MaNhanVien);
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                            dbAdapter.sqlCommand.Parameters.Add("@MaNhanVien", SqlDbType.NVarChar).Value = user.MaNhanVien;
                            dbAdapter.sqlCommand.Parameters.Add("@HinhAnhUrl", SqlDbType.NVarChar).Value = hinhanh;
                            dbAdapter.sqlCommand.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = user.PhoneNumber;
                            dbAdapter.sqlCommand.Parameters.Add("@DonViTraLuong_Id", SqlDbType.UniqueIdentifier).Value = user.DonViTraLuong_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = user.MaNhanVien;
                            dbAdapter.sqlCommand.Parameters.Add("@FullName", SqlDbType.NVarChar).Value = user.FullName;
                            dbAdapter.sqlCommand.Parameters.Add("@Email", SqlDbType.NVarChar).Value = user.Email;
                            dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = user.IsActive;
                            dbAdapter.sqlCommand.Parameters.Add("@SecurityStamp", SqlDbType.NVarChar).Value = SecurityStampRandom;
                            dbAdapter.sqlCommand.Parameters.Add("@PasswordHash", SqlDbType.NVarChar).Value = passwordhash;
                            var result = dbAdapter.runStoredNoneQuery();
                            Guid idct = Guid.NewGuid();
                            dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
                            dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = id;
                            dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = item.TapDoan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = item.DonVi_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = item.PhongBan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = item.BoPhan_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = item.ChucVu_Id;
                            dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
                            dbAdapter.runStoredNoneQuery();
                            dbAdapter.deConnect();
                            if (result == 0)
                            {
                                return StatusCode(StatusCodes.Status409Conflict, "Thật bất ngờ lỗi này tại hạ đỡ không được!");
                            }

                        }
                    }
                }
                return Ok("Thêm mới thành công.");
            }
            catch (Exception)
            {
                dbAdapter.deConnect();
                throw;
            }
        }

        [HttpPost("ExportFileExcel")]
        public ActionResult ExportFileExcel()
        {
            string fullFilePath = Path.Combine(environment.ContentRootPath, "Uploads/Templates/FileExportCBNV.xlsx");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    // Add a new worksheet if none exists
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Đơn vị"];
                if (worksheet == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("Đơn vị");
                }
                int stt = 1;
                int sublistIndex = 0;
                int indexrow = 4;
                string[] include = { "TapDoan" };
                var donvi = uow.DonVis.GetAll(x => !x.IsDeleted, null, include).ToList();
                foreach (var item in donvi)
                {
                    worksheet.InsertRow(indexrow, 1, indexrow);
                    worksheet.Row(indexrow).Height = 35;

                    worksheet.Cells["A" + indexrow].Value = stt;
                    worksheet.Cells["B" + indexrow].Value = item.MaDonVi;
                    worksheet.Cells["C" + indexrow].Value = item.TenDonVi;
                    worksheet.Cells["D" + indexrow].Value = item.TapDoan.TenTapDoan;
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet.Cells[indexrow, col];
                        var border = cell.Style.Border;
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 4)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet.Cells["A" + indexrow];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt++;
                    indexrow++;
                    sublistIndex++;

                }

                ExcelWorksheet worksheet2 = package.Workbook.Worksheets["Phòng ban"];
                if (worksheet2 == null)
                {
                    worksheet2 = package.Workbook.Worksheets.Add("Phòng ban");
                }
                int stt2 = 1;
                int indexrow2 = 4;
                string[] include2 = { "DonVi" };
                var phongban = uow.phongbans.GetAll(x => !x.IsDeleted, null, include2).ToList();
                foreach (var item in phongban)
                {
                    worksheet2.InsertRow(indexrow2, 1, indexrow2);
                    worksheet2.Row(indexrow2).Height = 20;

                    worksheet2.Cells["A" + indexrow2].Value = stt2;
                    worksheet2.Cells["B" + indexrow2].Value = item.MaPhongBan;
                    worksheet2.Cells["C" + indexrow2].Value = item.TenPhongBan;
                    worksheet2.Cells["D" + indexrow2].Value = item.DonVi.TenDonVi;
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet2.Cells[indexrow2, col];
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        cell.Style.WrapText = true;
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 4)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet2.Cells["A" + indexrow2];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt2++;
                    indexrow2++;
                }

                ExcelWorksheet worksheet3 = package.Workbook.Worksheets["Bộ phận"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet3 == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet3 = package.Workbook.Worksheets.Add("Bộ phận");
                }
                int stt3 = 1;
                int indexrow3 = 4;
                string[] include3 = { "Phongban" };
                var bophan = uow.BoPhans.GetAll(x => !x.IsDeleted, null, include3).ToList();
                foreach (var item in bophan)
                {
                    worksheet3.InsertRow(indexrow3, 1, indexrow3);
                    worksheet3.Row(indexrow3).Height = 20;
                    worksheet3.Cells["A" + indexrow3].Value = stt3;
                    worksheet3.Cells["B" + indexrow3].Value = item.MaBoPhan;
                    worksheet3.Cells["C" + indexrow3].Value = item.TenBoPhan;
                    worksheet3.Cells["D" + indexrow3].Value = item.Phongban.TenPhongBan;
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet3.Cells[indexrow3, col];
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        cell.Style.WrapText = true;
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 4)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet3.Cells["A" + indexrow3];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt3++;
                    indexrow3++;
                }

                ExcelWorksheet worksheet4 = package.Workbook.Worksheets["Chức vụ"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet4 == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet4 = package.Workbook.Worksheets.Add("Chức vụ");
                }
                int stt4 = 1;
                int indexrow4 = 4;
                var chucvu = uow.chucVus.GetAll(x => !x.IsDeleted).ToList();
                foreach (var item in chucvu)
                {
                    worksheet4.InsertRow(indexrow4, 1, indexrow4);
                    worksheet4.Row(indexrow4).Height = 20;

                    worksheet4.Cells["A" + indexrow4].Value = stt4;
                    worksheet4.Cells["B" + indexrow4].Value = item.MaChucVu;
                    worksheet4.Cells["C" + indexrow4].Value = item.TenChucVu;
                    for (int col = 1; col <= 3; col++)
                    {
                        var cell = worksheet4.Cells[indexrow4, col];
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        cell.Style.WrapText = true;
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 3)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet4.Cells["A" + indexrow4];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt4++;
                    indexrow4++;
                }

                ExcelWorksheet worksheet5 = package.Workbook.Worksheets["Tập đoàn"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet4 == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet5 = package.Workbook.Worksheets.Add("Tập đoàn");
                }
                int stt5 = 1;
                int indexrow5 = 4;
                var tapdoan = uow.tapDoans.GetAll(x => !x.IsDeleted).ToList();
                foreach (var item in tapdoan)
                {
                    worksheet5.InsertRow(indexrow5, 1, indexrow5);
                    worksheet5.Row(indexrow5).Height = 20;

                    worksheet5.Cells["A" + indexrow5].Value = stt5;
                    worksheet5.Cells["B" + indexrow5].Value = item.MaTapDoan;
                    worksheet5.Cells["C" + indexrow5].Value = item.TenTapDoan;
                    for (int col = 1; col <= 3; col++)
                    {
                        var cell = worksheet5.Cells[indexrow5, col];
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        cell.Style.WrapText = true;
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 3)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet5.Cells["A" + indexrow5];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt5++;
                    indexrow5++;
                }

                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }

        public class ExportListExcel_DanhSachCBNV
        {
            public string MaNhanVien { get; set; }
            public string Hoten { get; set; }
            public string Email { get; set; }
            public string SDT { get; set; }
            public string ChucVu { get; set; }
            public string BoPhan { get; set; }
            public string PhongBan { get; set; }
            public string DonVi { get; set; }
            public string DonViTraLuong { get; set; }

        }


        [HttpPost("ExportListExcel_DanhSachCBNV")]
        public ActionResult ExportFileExcel_DanhSachCBNV(List<ExportListExcel_DanhSachCBNV> data)
        {
            string fullFilePath = Path.Combine(environment.ContentRootPath, "Uploads/Templates/ExportDanhSachCBNV.xlsx");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int stt = 1;
                int startRow = 6;
                worksheet.Cells["C3"].Value = data[0].DonVi;
                foreach (var item in data)
                {
                    worksheet.Cells["A" + startRow].Value = stt;
                    worksheet.Cells["B" + startRow].Value = item.MaNhanVien;
                    worksheet.Cells["C" + startRow].Value = item.Hoten;
                    worksheet.Cells["D" + startRow].Value = item.Email;
                    worksheet.Cells["E" + startRow].Value = item.SDT;
                    worksheet.Cells["F" + startRow].Value = item.ChucVu;
                    worksheet.Cells["G" + startRow].Value = item.BoPhan;
                    worksheet.Cells["H" + startRow].Value = item.PhongBan;
                    worksheet.Cells["I" + startRow].Value = item.DonViTraLuong;
                    for (int col = 1; col <= 9; col++)
                    {
                        var cell = worksheet.Cells[startRow, col];
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col > 5 || col < 10)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        else
                        {
                            cell.Style.WrapText = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                    var cellA = worksheet.Cells["A" + startRow];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Row(startRow).Height = 30;
                    ++stt;
                    ++startRow;
                }

                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }

        [HttpGet("list-user-administrator")]
        public ActionResult GetListUserAdministrator(int page = 1, string keyword = null, Guid? donviId = null)
        {
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).FirstOrDefault();
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetListUserAdministrator");
            dbAdapter.sqlCommand.Parameters.Add("@keyword", SqlDbType.NVarChar).Value = keyword;
            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = donviId;
            var result = dbAdapter.runStored2ObjectList();
            dbAdapter.deConnect();
            int totalRow = result.Count();
            int pageSize = pageSizeData.PageSize;
            int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);
            if (page < 1)
            {
                page = 1;
            }
            else if (page > totalPage)
            {
                page = totalPage;
            }

            var datalist = result.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new
            {
                totalRow,
                totalPage,
                pageSize,
                datalist
            });
        }

        [HttpGet("user-administrator")]
        public ActionResult GetUserAdministratorById(Guid? id = null, Guid? role_Id = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetUserAdministratorById");
            dbAdapter.sqlCommand.Parameters.Add("@user_Id", SqlDbType.UniqueIdentifier).Value = id;
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = role_Id;
            var result = dbAdapter.runStored2Object();
            dbAdapter.deConnect();
            return Ok(result);
        }

        [HttpPost("user-administrator")]
        public ActionResult PostUserAdmin(Guid id, List<ClassAdminActive> model)
        {
            dbAdapter.connect();
            foreach (var item in model)
            {
                dbAdapter.createStoredProceder("sp_GetRoleAdmin");
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                var result = dbAdapter.runStored2Object();
                if (result is ExpandoObject expandoObj && expandoObj.Count() <= 0)
                {
                    dbAdapter.deConnect();
                    return StatusCode(StatusCodes.Status409Conflict, "Vai trò phải là administrator!");
                }
            }
            foreach (var item in model)
            {
                dbAdapter.createStoredProceder("sp_GetCheckExistsRole");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = item.Id;
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                var result = dbAdapter.runStored2Object();
                if (result is ExpandoObject expandoObj && expandoObj.Count() > 0)
                {
                    dbAdapter.deConnect();
                    return StatusCode(StatusCodes.Status409Conflict, "Vai trò này đã có ở người dùng này!");
                }
            }
            /*                dbAdapter.createStoredProceder("sp_DeleteRoleAdmin");
                            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
                            dbAdapter.runStoredNoneQuery();*/
            foreach (var item in model)
            {
                dbAdapter.createStoredProceder("sp_PostUserAdmin");
                dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = item.Id;
                dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = item.Role_Id;
                dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = item.IsActive;
                var result = dbAdapter.runStoredNoneQuery();
            }
            dbAdapter.deConnect();
            return Ok("Thêm mới vai trò thành công!");
        }

        [HttpPut("user-administrator")]
        public ActionResult PutUserAdmin(ClassAdminActive model)
        {
            dbAdapter.connect();

            dbAdapter.createStoredProceder("sp_GetRoleAdmin");
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = model.Role_Id;
            var result = dbAdapter.runStored2Object();
            if (result is ExpandoObject expandoObj && expandoObj.Count() <= 0)
            {
                dbAdapter.deConnect();
                return StatusCode(StatusCodes.Status409Conflict, "Vai trò phải là administrator!");
            }

            dbAdapter.createStoredProceder("sp_GetCheckExistsRole");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = model.Role_Id;
            var result2 = dbAdapter.runStored2Object();
            if (result2 is ExpandoObject expandoObj2 && expandoObj2.Count() > 0)
            {
                dbAdapter.deConnect();
                return StatusCode(StatusCodes.Status409Conflict, "Vai trò đã có ở người dùng này!");
            }

            dbAdapter.createStoredProceder("sp_DeleteRoleAdmin");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = model.Roleold_Id;
            dbAdapter.runStoredNoneQuery();

            dbAdapter.createStoredProceder("sp_PostUserAdmin");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = model.Id;
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = model.Role_Id;
            dbAdapter.sqlCommand.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;
            dbAdapter.runStoredNoneQuery();

            dbAdapter.deConnect();
            return Ok("Chỉnh sửa vai trò thành công!");
        }

        [HttpDelete("user-administrator")]
        public ActionResult DeleteUserAdmin(Guid id, Guid role_Id)
        {
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_DeleteRoleAdmin");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = id;
            dbAdapter.sqlCommand.Parameters.Add("@role_Id", SqlDbType.UniqueIdentifier).Value = role_Id;
            dbAdapter.runStoredNoneQuery();
            dbAdapter.deConnect();
            return Ok("Xóa vai trò thành công!");
        }


        [HttpPost("add-don-vi-cbnv")]
        public ActionResult PostAddDonViCBNV(ClassAddDonViCBNV model)
        {
            if (uow.chiTiet_DV_PB_BPs.Exists(x => x.User_Id == model.User_Id && x.DonVi_Id == model.DonVi_Id && x.PhongBan_Id == model.PhongBan_Id && x.BoPhan_Id == model.BoPhan_Id && x.ChucVu_Id == model.ChucVu_Id))
            {
                return StatusCode(StatusCodes.Status409Conflict, "CBNV đã tồn tại ở đơn vị hoặc phòng ban hoặc bộ phận!");
            }
            dbAdapter.connect();
            Guid idct = Guid.NewGuid();
            dbAdapter.createStoredProceder("sp_PostChiTiet_DV_PB_BP");
            dbAdapter.sqlCommand.Parameters.Add("@id", SqlDbType.UniqueIdentifier).Value = idct;
            dbAdapter.sqlCommand.Parameters.Add("@User_Id", SqlDbType.UniqueIdentifier).Value = model.User_Id;
            dbAdapter.sqlCommand.Parameters.Add("@TapDoan_Id", SqlDbType.UniqueIdentifier).Value = model.TapDoan_Id;
            dbAdapter.sqlCommand.Parameters.Add("@DonVi_Id", SqlDbType.UniqueIdentifier).Value = model.DonVi_Id;
            dbAdapter.sqlCommand.Parameters.Add("@PhongBan_Id", SqlDbType.UniqueIdentifier).Value = model.PhongBan_Id;
            dbAdapter.sqlCommand.Parameters.Add("@BoPhan_Id", SqlDbType.UniqueIdentifier).Value = model.BoPhan_Id;
            dbAdapter.sqlCommand.Parameters.Add("@ChucVu_Id", SqlDbType.UniqueIdentifier).Value = model.ChucVu_Id;
            dbAdapter.sqlCommand.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = Guid.Parse(User.Identity.Name);
            dbAdapter.runStoredNoneQuery();
            dbAdapter.deConnect();
            return Ok("Thêm mới thành công!");
        }

        [HttpDelete("delete-don-vi-cbnv")]
        public ActionResult DeleteDonViCBNV(Guid chiTiet_Id, Guid user_Id, Guid tapDoan_Id, Guid donVi_Id)
        {
            dbAdapter.connect();
            dbAdapter.createStoredProceder("sp_GetCheckExistsRoleForDeleteCBNVDonVi");
            dbAdapter.sqlCommand.Parameters.Add("@user_Id", SqlDbType.UniqueIdentifier).Value = user_Id;
            dbAdapter.sqlCommand.Parameters.Add("@donvi_Id", SqlDbType.UniqueIdentifier).Value = donVi_Id;
            dbAdapter.sqlCommand.Parameters.Add("@tapDoan_Id", SqlDbType.UniqueIdentifier).Value = tapDoan_Id;
            var result2 = dbAdapter.runStored2Object();
            if (result2 is ExpandoObject expandoObj2 && expandoObj2.Count() > 0)
            {
                dbAdapter.deConnect();
                return StatusCode(StatusCodes.Status409Conflict, "Vai trò đã có ở người dùng này!");
            }

            dbAdapter.createStoredProceder("sp_DeleteAllChiTietDonViForCBNV");
            dbAdapter.sqlCommand.Parameters.Add("@chiTiet_Id", SqlDbType.UniqueIdentifier).Value = chiTiet_Id;
            dbAdapter.sqlCommand.Parameters.Add("@user_Id", SqlDbType.UniqueIdentifier).Value = user_Id;
            dbAdapter.sqlCommand.Parameters.Add("@donvi_Id", SqlDbType.UniqueIdentifier).Value = donVi_Id;
            dbAdapter.sqlCommand.Parameters.Add("@tapDoan_Id", SqlDbType.UniqueIdentifier).Value = tapDoan_Id;
            dbAdapter.runStoredNoneQuery();
            dbAdapter.deConnect();
            return Ok("Xóa thành công!");
        }
    }
}