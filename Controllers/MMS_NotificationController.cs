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

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_NotificationController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        private readonly PushThongBao _thongbao;

        private readonly HttpClient _httpClient;
        private static readonly string Url_push = "https://fcm.googleapis.com/v1/projects/bms-mobi/messages:send";
        public MMS_NotificationController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment,
        PushThongBao thongbao, DataService master, HttpClient httpClient)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
            _thongbao = thongbao;

            _httpClient = httpClient;
        }


        [HttpPost("PushThongBao")]
        public async Task SendPushNotificationAsync(string body, Guid? listIds)
        {
            // var claims = User.Claims;
            // var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var dataList = await _master.GetToken();
            var tokensToSend = new List<string>();
            //  var nguoiPhuTrach = uow.MMS_PhuTrachBoPhans.GetSingle(x => !x.IsDeleted && x.PhuongTien_Id == listIds);

            if (body.Contains("yêu cầu bảo dưỡng phương tiện") && !body.Contains("xác nhận") && !body.Contains("huỷ"))
            {
                var relatedTokens = dataList.Where(x => !x.IsDeleted && x.LoaiDieuPhoi == "5")
        // var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == NguoiPhuTrach.User_Id)
        .Select(x => x.FCMToken)
        .ToList();
                tokensToSend.AddRange(relatedTokens);
            }
            if (body.Contains("vừa được xác nhận 1 yêu cầu bảo dưỡng phương tiện")
            || body.Contains("vừa bị huỷ xác nhận 1 yêu cầu bảo dưỡng phương tiện")
            || body.Contains("vừa bị huỷ 1 yêu cầu bảo dưỡng phương tiện"))
            {
                var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == Guid.Parse("ac9ead22-e0e7-488c-92dc-7dbdf180e027"))
       // var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == NguoiPhuTrach.User_Id)
       .Select(x => x.FCMToken)
       .ToList();
                tokensToSend.AddRange(relatedTokens);
            }
            if (body.Contains("lệnh hoàn thành bảo dưỡng phương tiện"))
            {
                var relatedTokens = dataList.Where(x => !x.IsDeleted && x.LoaiDieuPhoi == "5")
       // var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == NguoiPhuTrach.User_Id)
       .Select(x => x.FCMToken)
       .ToList();
                tokensToSend.AddRange(relatedTokens);
            }
            if (body.Contains("vừa được xác nhận 1 đề xuất hoàn thành bảo dưỡng phương tiện") || body.Contains("vừa bị huỷ xác nhận 1 hoàn thành bảo dưỡng phương tiện"))
            {
                var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == Guid.Parse("ac9ead22-e0e7-488c-92dc-7dbdf180e027"))
       // var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == NguoiPhuTrach.User_Id)
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
                            listIds = listIds // Chuyển List<Guid?> thành JSON string
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

        [HttpPost("ThongBaoNhapKM")]
        public async Task SendPushNotificationNhapKMAsync()
        {
            var claims = User.Claims;
            var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var dataList = await _master.GetToken();
            var tokensToSend = new List<string>();

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
                            body = "Vui lòng nhập số KM vào "
                        },
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


    }
}
