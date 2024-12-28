using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ThacoLibs;
using static ERP.Data.MyDbContext;
using System.Data;
using Microsoft.Extensions.Configuration;
using Konscious.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;

namespace ERP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class __TestController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DbAdapter dbAdapter;
        private readonly IConfiguration configuration;

        public __TestController(IConfiguration _configuration, IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            configuration = _configuration;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            //string connectionString = ConfigurationManager.ConnectionStrings["ConnectionStrings"].ConnectionString;
            dbAdapter = new DbAdapter(connectionString);
        }

        [HttpGet]
        public ActionResult Get(string keyword)
        {
            return Ok("OK, data = " + keyword);
        }

        [HttpPost("")]
        public IActionResult Post(string keyword)
        {
            string password = keyword; // Mật khẩu của người dùng
            byte[] salt = new byte[128]; // Salt ngẫu nhiên

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8; // Số lượng luồng xử lý song song
                argon2.MemorySize = 65536; // Kích thước bộ nhớ sử dụng (trong KiB)
                argon2.Iterations = 4; // Số lần lặp lại

                byte[] hashBytes = argon2.GetBytes(128); // 32 bytes là kích thước đầu ra mong muốn

                string hashedPassword = Convert.ToBase64String(hashBytes);
                return Ok("OK, data = " + hashedPassword);
            }
        }
        [HttpPost("test-password")]
        public IActionResult Posttestpassword(string keyword)
        {
            var password = Commons.HashPassword(keyword);
            var passwordhash = Commons.HashPassword(password);
            return Ok(passwordhash);
        }
        [HttpGet("get-types")]
        public ActionResult GetTypes()
        {
            List<string> types = new List<string> { "string", "number", "datetime", "boolean" };
            return Ok(types);
        }
        [HttpGet("get-entity-table")]
        public ActionResult entity_table()
        {
            List<string> types = new List<string> { "PhieuMuaHang", "ChiTietPhieuMuaHang", "PhieuKiemKe", "ChiTietPhieuKiemKe",
                                                    "PhieuNhap","ChiTietPhieuNhap", "PhieuXuat", "ChiTietPhieuXuat","PhieuThanhLy", "ChiTietPhieuXuat", "ThongTinSanPham", 
                                                    "ChiTietKho", "SoDuDauKy","ChiTietKho", "PhieuNhapThanhPham","ChiTietPhieuNhapThanhPham","PhieuXuatThanhPham","ChiTietPhieuXuatThanhPham",
                                                    "DinhMucVatTu","ChiTietDinhMucVatTu","NangLucSanXuat","KeHoach","ChiTietKeHoach","PhieuDeNghi", "ChiTietPhieuDeNghi",
                                                    "TheoDoiDonHang", "ChiTietDonHang"};
            return Ok(types);
        }
    }
}