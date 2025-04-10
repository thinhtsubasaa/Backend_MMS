using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using OfficeOpenXml;
using Microsoft.CodeAnalysis;
using ERP.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using static ERP.Data.MyDbContext;
using System.Data;
using Microsoft.AspNetCore.Http;
using ERP.Helpers;
using System.Net.Http;
using DocumentFormat.OpenXml.Presentation;
using System.Threading.Tasks;
using ERP.Data.Migrations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using ERP.UOW;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AutomaticController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly HttpClient _httpClient;
        // private readonly MyTypedAdsun Adsun;

        public AutomaticController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, HttpClient httpClient)
        {
            uow = _uow;
            environment = _environment;
            userManager = _userManager;
            _httpClient = httpClient;
        }
        public interface IMMSNotificationService
        {
            Task SendPushNotificationNhapKMAsync();
        }

        public class MMSNotificationService : IMMSNotificationService
        {
            private readonly IUnitofWork uow;
            private readonly DataService _master;
            private readonly PushThongBao _thongbao;
            private readonly HttpClient _httpClient;
            private static readonly string Url_push = "https://fcm.googleapis.com/v1/projects/bms-mobi/messages:send";

            public MMSNotificationService(IUnitofWork _uow, DataService master, PushThongBao thongbao, HttpClient httpClient)
            {
                uow = _uow;
                _master = master;
                _thongbao = thongbao;
                _httpClient = httpClient;
            }

            public async Task SendPushNotificationNhapKMAsync()
            {
                var dataList = await _master.GetToken();
                var phuongtien = uow.MMS_PhuTrachBoPhans.GetAll(x => !x.IsDeleted, x => x.OrderByDescending(x => x.CreatedDate));

                var tokensToSend = new List<string>();

                var relatedTokens = dataList.Where(x => !x.IsDeleted && x.User_Id == Guid.Parse("ac9ead22-e0e7-488c-92dc-7dbdf180e027"))
              .Select(x => x.FCMToken)
              .ToList();
                tokensToSend.AddRange(relatedTokens);

                var accessToken = await _thongbao.GetAccessTokenAsync();
                var tokensToSendDistinct = tokensToSend.Distinct().ToList();

                foreach (var token in tokensToSendDistinct)
                {
                    var user = dataList.Where(x => !x.IsDeleted && x.FCMToken == token).FirstOrDefault();
                    var fullName = user != null ? user?.NguoiPhuTrach.Split('-')[0].Trim() : "";
                    var phuongTien_Id = user != null ? phuongtien.Where(x => x.User_Id == user.User_Id)?.FirstOrDefault()?.PhuongTien_Id : null;

                    var payload = new
                    {
                        message = new
                        {
                            token = token,
                            notification = new
                            {
                                title = $"Xin chào, {fullName}",
                                body = "Vui lòng nhập số KM"
                            },
                            data = new
                            {
                                type = "xac_nhan",
                                listIds = phuongTien_Id // Chuyển List<Guid?> thành JSON string
                            }
                        }
                    };

                    var request = new HttpRequestMessage(HttpMethod.Post, Url_push)
                    {
                        Headers = { Authorization = new AuthenticationHeaderValue("Bearer", accessToken) },
                        Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                    };

                    var response = await _httpClient.SendAsync(request);
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

        public class MasterDataService : IHostedService, IDisposable
        {
            private Timer _timer;
            private readonly IServiceProvider _serviceProvider;

            public MasterDataService(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                var now = DateTime.Now;

                // Đặt thời gian chạy đầu tiên vào 23h30 hôm nay hoặc ngày mai nếu đã qua 23h30
                var nextRunTime = now.Date.Add(new TimeSpan(8, 30, 0));
                if (now > nextRunTime)
                {
                    nextRunTime = nextRunTime.AddDays(1);
                }
                // Khoảng thời gian chờ đến lần chạy đầu tiên
                var firstInterval = nextRunTime - now;
                Console.WriteLine($"[INFO] Hẹn lịch chạy đầu tiên vào: {nextRunTime}");
                _timer = new Timer(DoWork, null, firstInterval, TimeSpan.FromDays(1));
                // Đặt thời gian chạy định kỳ mỗi giờ
                // _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(1));

                return Task.CompletedTask;
            }

            private async void DoWork(object state)
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dataService = scope.ServiceProvider.GetRequiredService<IMMSNotificationService>();
                    await dataService.SendPushNotificationNhapKMAsync();

                }
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                _timer?.Change(Timeout.Infinite, 0);
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                _timer?.Dispose();
            }


        }

    }
}


