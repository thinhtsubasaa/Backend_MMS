using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ERP.Helpers;
using ERP.Infrastructure;
using ERP.Models;
using static ERP.Data.MyDbContext;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System.Security.AccessControl;
using ThacoLibs;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using static ERP.Helpers.MyTypedClient;
using System.IO;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Route("token")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly AppSettings appSettings;
        private readonly IConfiguration config;
        private readonly IConfiguration configuration;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly DbAdapter dbAdapter;
        private readonly DownloadImage _image;

        // private readonly MyTypedClient client;
        public AuthController(IConfiguration _configuration, RoleManager<ApplicationRole> _roleManager, IUnitofWork _uow, UserManager<ApplicationUser> _userManager, SignInManager<ApplicationUser> _signInManager, IOptions<AppSettings> _appSettings, IConfiguration _config, DownloadImage image)
        {
            uow = _uow;
            userManager = _userManager;
            signInManager = _signInManager;
            appSettings = _appSettings.Value;
            config = _config;
            _image = image;

            roleManager = _roleManager;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            dbAdapter = new DbAdapter(connectionString);
        }
        [HttpPost]
        public async Task<IActionResult> Authencation(LoginModel model)
        {
            var user = new ApplicationUser();
            var Email = "";
            var passwordhash = Commons.HashPassword(model.Password);
            if (!string.IsNullOrEmpty(model.Domain))
            {
                string checkemail = model.Username + "@" + model.Domain;
                var appUser = userManager.Users.Where(x => x.Email == checkemail).FirstOrDefault();
                if (appUser == null)
                {
                    return BadRequest("Tài khoản không tồn tại");
                }
                else if (!appUser.IsActive)
                {
                    return BadRequest("Tài khoản đã bị khóa");
                }
                var role = await userManager.GetRolesAsync(appUser);

                if (role.Contains("Administrator"))
                {
                    Email = model.Username + "@" + model.Domain;
                    user = userManager.Users.Where(x => x.Email == checkemail).FirstOrDefault();
                    var result = CheckLogin(model.Username, model.Password, model.Domain);
                    if (result)
                    {
                        return Ok(
                            GenToken(new UserToken { Id = user.Id.ToString(), Email = user.Email, FullName = user.FullName, MustChangePass = user.MustChangePass, AccessRole = "Administrator", HinhAnhUrl = user.HinhAnhUrl, }));
                    }
                    else
                    {
                        return BadRequest("Thông tin đăng nhập không đúng");
                    }
                }
                else if (role.Contains("IT"))
                {
                    Email = model.Username + "@" + model.Domain;
                    user = userManager.Users.Where(x => x.Email == checkemail).FirstOrDefault();
                    var result = CheckLogin(model.Username, model.Password, model.Domain);
                    if (result)
                    {
                        return Ok(GenToken(new UserToken { Id = user.Id.ToString(), Email = user.Email, FullName = user.FullName, MustChangePass = user.MustChangePass, AccessRole = "IT", HinhAnhUrl = user.HinhAnhUrl, }));
                    }
                    else
                    {
                        return BadRequest("Thông tin đăng nhập không đúng");
                    }
                }
                else
                {
                    // Email = model.Username + "@" + model.Domain;
                    user = userManager.Users.Where(x => x.Email == checkemail).FirstOrDefault();
                    var result = CheckLogin(model.Username, model.Password, model.Domain);
                    if (result)
                    {
                        return Ok(GenToken(new UserToken { Id = user.Id.ToString(), Email = user.Email, FullName = user.FullName, MustChangePass = user.MustChangePass, AccessRole = roleManager.Roles.Where(x => role.Contains(x.Name)).ToString(), HinhAnhUrl = user.HinhAnhUrl, }));
                    }
                    else
                    {
                        return BadRequest("Thông tin đăng nhập không đúng");
                    }
                }

            }
            else
            {
                user = userManager.Users.FirstOrDefault(x => x.UserName == model.Username);
                if (user == null)
                {
                    return BadRequest("Tài khoản không tồn tại");
                }
                var role = await userManager.GetRolesAsync(user);
                if (role.Contains("Administrator"))
                {
                    var checklogin = Commons.VerifyPassword(model.Password, user.PasswordHash);
                    if (checklogin == false)
                    {
                        return BadRequest("Mật khẩu tài khoản không đúng");
                    }
                    if (checklogin == true)
                    {
                        return Ok(GenToken(new UserToken { Id = user.Id.ToString(), Email = user.Email, FullName = user.FullName, MustChangePass = user.MustChangePass, AccessRole = "Administrator", HinhAnhUrl = user.HinhAnhUrl, }));
                    }
                    else
                    {
                        return BadRequest("Mật khẩu tài khoản không đúng");
                    }
                }
                else if (role.Contains("IT"))
                {
                    var checklogin = Commons.VerifyPassword(model.Password, user.PasswordHash);
                    if (checklogin == false)
                    {
                        return BadRequest("Mật khẩu tài khoản không đúng");
                    }
                    if (checklogin == true)
                    {
                        return Ok(GenToken(new UserToken { Id = user.Id.ToString(), Email = user.Email, FullName = user.FullName, MustChangePass = user.MustChangePass, AccessRole = "IT", HinhAnhUrl = user.HinhAnhUrl, }));
                    }
                    else
                    {
                        return BadRequest("Mật khẩu tài khoản không đúng");
                    }
                }
                else
                {
                    var checklogin = Commons.VerifyPassword(model.Password, user.PasswordHash);
                    if (checklogin == false)
                    {
                        return BadRequest("Mật khẩu tài khoản không đúng");
                    }
                    if (checklogin == true)
                    {
                        return Ok(GenToken(new UserToken { Id = user.Id.ToString(), Email = user.Email, FullName = user.FullName, MustChangePass = user.MustChangePass, AccessRole = roleManager.Roles.Where(x => role.Contains(x.Name)).ToString(), HinhAnhUrl = user.HinhAnhUrl, }));
                    }
                    else
                    {
                        return BadRequest("Mật khẩu tài khoản không đúng");
                    }
                }
            }
        }
        private InfoLogin GenToken(UserToken userToken)
        {
            var appUser = userManager.FindByIdAsync(userToken.Id);
            var userhinhanh = userManager.FindByIdAsync(userToken.Id).Result;
            var roles = userManager.GetRolesAsync(appUser.Result);
            var role = roles.GetAwaiter().GetResult();
            var user = userManager.Users.FirstOrDefaultAsync(u => u.FullName == userToken.FullName).Result;
            // HttpClient httpClient = new HttpClient();
            // MyTypedClient nhanvienService = new MyTypedClient(httpClient);
            // NhanVienHRMModel app_user = nhanvienService.ThongTinNhanVien(userhinhanh.MaNhanVien);
            string[] included = { "CongBaoVe" };


            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, userToken.Id),

                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var accessRole = string.Empty;
            if (role.Count > 0)
            {
                accessRole = role[0]; // Lấy vai trò đầu tiên từ danh sách roles
            }
            var token = tokenHandler.CreateToken(tokenDescriptor);
            _ = CallApiMMS(tokenHandler.WriteToken(token));


            return new InfoLogin()
            {
                Token = tokenHandler.WriteToken(token),
                Id = userToken.Id,
                Email = userToken.Email,
                FullName = userToken.FullName,
                Expires = token.ValidTo,
                MustChangePass = userToken.MustChangePass,
                AccessRole = accessRole,
                HinhAnhUrl = _image.ImageHRM(userhinhanh.HinhAnhUrl),
                QrCode = Commons.EnCode(userhinhanh.MaNhanVien),
                MaNhanVien = userhinhanh.MaNhanVien,
                TenPhongBan = userhinhanh?.TenPhongBan,
                ChucDanh = userhinhanh?.ChucDanh,
                ChucVu = userhinhanh?.ChucVu,
                TrangThai = userhinhanh?.TrangThai,
                ChuoiPhongBan = userhinhanh?.ChuoiPhongBan,

            };
        }

        private async System.Threading.Tasks.Task CallApiMMS(string token)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    // Gọi API 2
                    var response = await client.GetAsync(Commons.ApiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Success from API 2: " + response.StatusCode);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to call API 2: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        [Authorize]
        [HttpPost("Refresh")]
        public IActionResult RefreshToken(UserToken model)
        {
            var appUser = userManager.FindByIdAsync(model.Id);
            var userhinhanh = userManager.FindByIdAsync(model.Id).Result;
            var roles = userManager.GetRolesAsync(appUser.Result);
            var role = roles.GetAwaiter().GetResult();
            var accessRole = string.Empty;
            if (role.Count > 0)
            {
                accessRole = role[0]; // Lấy vai trò đầu tiên từ danh sách roles
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(GenToken(new UserToken { Id = model.Id, Email = model.Email, FullName = model.FullName, MustChangePass = model.MustChangePass, AccessRole = accessRole, HinhAnhUrl = userhinhanh.HinhAnhUrl }));
        }
        private static bool CheckLogin(string email, string password, string domain)
        {
            ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013);
            service.Credentials = new WebCredentials(email, password);
            service.Url = new Uri("https://mail.thaco.com.vn/ews/exchange.asmx");
            try
            {
                // Tìm kiếm một số mục trong hộp thư
                FindItemsResults<Item> findResults = service.FindItems(WellKnownFolderName.Inbox, new ItemView(1)).Result;
                // Nếu danh sách mục tìm thấy không rỗng, đăng nhập thành công
                if (findResults.Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Nếu có ngoại lệ ném ra, đăng nhập không thành công
                Console.WriteLine(ex.Message);
                return false;
            }
            //ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013);
            //service.Credentials = new WebCredentials(email, password);
            //if (domain == "thaco.com.vn")
            //{
            //  service.Url = new Uri("https://mail.thaco.com.vn/ews/exchange.asmx");
            //}
            //else if (domain == "vinamazda.vn")
            //{
            //  service.Url = new Uri("https://mail.vinamazda.vn/ews/exchange.asmx");
            //}
            //else if (domain == "dqmcorp.vn")
            //{
            //  service.Url = new Uri("https://mail.dqmcorp.vn/ews/exchange.asmx");
            //}
            //try
            //{
            //  var findFolderResults = service.FindFolders(WellKnownFolderName.Root, new FolderView(1));
            //  if (findFolderResults != null)
            //    return true;
            //  else
            //    return false;
            //}
            //catch
            //{
            //  return false;
            //}

        }
    }
}