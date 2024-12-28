// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.IO;
// using System.Linq;
// using System.Linq.Expressions;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Cors;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc;
// using ERP.Infrastructure;
// using ERP.Models;
// using OfficeOpenXml;
// using Microsoft.CodeAnalysis;
// using ERP.Attributes;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.AspNetCore.Identity;
// using static ERP.Data.MyDbContext;
// using System.Data;
// using Microsoft.AspNetCore.Http;
// using ERP.Helpers;
// using System.Net.Http;
// using DocumentFormat.OpenXml.Presentation;
// using System.Threading.Tasks;
// using ERP.Data.Migrations;
// using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
// using Microsoft.Extensions.Hosting;
// using System.Threading;
// using Microsoft.Extensions.DependencyInjection;
// using ERP.UOW;
// namespace ERP.Controllers
// {
//     [EnableCors("CorsApi")]
//     [Authorize]
//     [Route("api/[controller]")]
//     [ApiController]
//     public class AdsunController : ControllerBase
//     {
//         private readonly IUnitofWork uow;
//         private readonly UserManager<ApplicationUser> userManager;
//         public static IWebHostEnvironment environment;
//         private readonly HttpClient _httpClient;
//         // private readonly MyTypedAdsun Adsun;

//         public AdsunController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, HttpClient httpClient)
//         {
//             uow = _uow;
//             environment = _environment;
//             userManager = _userManager;
//             _httpClient = httpClient;
//         }


//         public class VehicleDataUpdateService : IHostedService, IDisposable
//         {
//             private Timer _timer;
//             private readonly IServiceProvider _serviceProvider;

//             public VehicleDataUpdateService(IServiceProvider serviceProvider)
//             {
//                 _serviceProvider = serviceProvider;
//             }

//             public Task StartAsync(CancellationToken cancellationToken)
//             {
//                 // Đặt thời gian chạy định kỳ mỗi giờ
//                 _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
//                 // _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
//                 return Task.CompletedTask;
//             }

//             private async void DoWork(object state)
//             {
//                 using (var scope = _serviceProvider.CreateScope())
//                 {
//                     var vehicleService = scope.ServiceProvider.GetRequiredService<VehicleService>();
//                     var uow = scope.ServiceProvider.GetRequiredService<IUnitofWork>();

//                     var vehicleList = await vehicleService.GetVehicleInfoAsync();

//                     if (vehicleList != null && vehicleList.Count > 0)
//                     {
//                         foreach (var vehicle in vehicleList)
//                         {
//                             var existingVehicle = uow.Adsuns.GetAll(v => v.Plate == vehicle.Plate).FirstOrDefault();

//                             if (existingVehicle != null)
//                             {
//                                 // Cập nhật dữ liệu nếu biển số đã tồn tại
//                                 existingVehicle.GroupId = vehicle.GroupId;
//                                 existingVehicle.GroupName = vehicle.GroupName;
//                                 existingVehicle.Lat = vehicle.Lat.ToString();
//                                 existingVehicle.Lng = vehicle.Lng.ToString();
//                                 existingVehicle.Speed = vehicle.Speed.ToString();
//                                 existingVehicle.Km = vehicle.Km.ToString();
//                                 existingVehicle.Gsm = vehicle.Gsm;
//                                 existingVehicle.Gps = vehicle.Gps;
//                                 existingVehicle.Key = vehicle.Key;
//                                 existingVehicle.Door = vehicle.Door;
//                                 existingVehicle.Temper = vehicle.Temper;
//                                 existingVehicle.Temper2 = vehicle.Temper2;
//                                 existingVehicle.Fuel = vehicle.Fuel.ToString();
//                                 existingVehicle.DriverName = vehicle.DriverName;
//                                 existingVehicle.Liciense = vehicle.Liciense;
//                                 existingVehicle.TimeUpdate = vehicle.TimeUpdate;
//                                 existingVehicle.Address = vehicle.Address;
//                                 existingVehicle.InputPower = vehicle.InputPower.ToString();
//                                 existingVehicle.TripKm = vehicle.TripKm.ToString();
//                                 existingVehicle.IsStop = vehicle.IsStop;
//                                 existingVehicle.StopTime = vehicle.StopTime;
//                                 existingVehicle.StopCounter = vehicle.StopCounter.ToString();
//                                 existingVehicle.Angle = vehicle.Angle.ToString();
//                                 existingVehicle.ACOnOff = vehicle.ACOnOff;
//                                 existingVehicle.IsOverSpeed = vehicle.IsOverSpeed;
//                                 existingVehicle.OverSpeedCount = vehicle.OverSpeedCount.ToString();
//                                 existingVehicle.BeginStop = vehicle.BeginStop;
//                                 existingVehicle.DayDrivingTime = vehicle.DayDrivingTime.ToString();
//                                 existingVehicle.DrivingTime = vehicle.DrivingTime.ToString();
//                                 existingVehicle.Over10h = vehicle.Over10h;
//                                 existingVehicle.Over4h = vehicle.Over4h;
//                                 existingVehicle.VehicleType = vehicle.VehicleType;
//                                 existingVehicle.SheeatsOrTons = vehicle.SheeatsOrTons;
//                                 existingVehicle.UpdatedDate = DateTime.Now;
//                                 // Replace with the actual user ID

//                                 uow.Adsuns.Update(existingVehicle);
//                             }
//                             else
//                             {
//                                 // Thêm mới nếu biển số chưa tồn tại
//                                 var LsAdsun = new Adsun
//                                 {
//                                     Id = vehicle.Id,
//                                     Plate = vehicle.Plate,
//                                     GroupId = vehicle.GroupId,
//                                     GroupName = vehicle.GroupName,
//                                     Lat = vehicle.Lat.ToString(),
//                                     Lng = vehicle.Lng.ToString(),
//                                     Speed = vehicle.Speed.ToString(),
//                                     Km = vehicle.Km.ToString(),
//                                     Gsm = vehicle.Gsm,
//                                     Gps = vehicle.Gps,
//                                     Key = vehicle.Key,
//                                     Door = vehicle.Door,
//                                     Temper = vehicle.Temper,
//                                     Temper2 = vehicle.Temper2,
//                                     Fuel = vehicle.Fuel.ToString(),
//                                     DriverName = vehicle.DriverName,
//                                     Liciense = vehicle.Liciense,
//                                     TimeUpdate = vehicle.TimeUpdate,
//                                     Address = vehicle.Address,
//                                     InputPower = vehicle.InputPower.ToString(),
//                                     TripKm = vehicle.TripKm.ToString(),
//                                     IsStop = vehicle.IsStop,
//                                     StopTime = vehicle.StopTime,
//                                     StopCounter = vehicle.StopCounter.ToString(),
//                                     Angle = vehicle.Angle.ToString(),
//                                     ACOnOff = vehicle.ACOnOff,
//                                     IsOverSpeed = vehicle.IsOverSpeed,
//                                     OverSpeedCount = vehicle.OverSpeedCount.ToString(),
//                                     BeginStop = vehicle.BeginStop,
//                                     DayDrivingTime = vehicle.DayDrivingTime.ToString(),
//                                     DrivingTime = vehicle.DrivingTime.ToString(),
//                                     Over10h = vehicle.Over10h,
//                                     Over4h = vehicle.Over4h,
//                                     VehicleType = vehicle.VehicleType,
//                                     SheeatsOrTons = vehicle.SheeatsOrTons,
//                                     CreatedDate = DateTime.Now,

//                                 };

//                                 uow.Adsuns.Add(LsAdsun);
//                             }
//                         }

//                         // Lưu tất cả các thay đổi vào cơ sở dữ liệu
//                         uow.Complete();
//                     }
//                 }
//             }



//             public Task StopAsync(CancellationToken cancellationToken)
//             {
//                 _timer?.Change(Timeout.Infinite, 0);
//                 return Task.CompletedTask;
//             }

//             public void Dispose()
//             {
//                 _timer?.Dispose();
//             }


//         }

//     }
// }



using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ERP.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static ERP.Data.MyDbContext;
using ERP.Helpers;
using ERP.Models;
using Google;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdsunController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly HttpClient _httpClient;

        public AdsunController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, HttpClient httpClient)
        {
            uow = _uow;
            environment = _environment;
            userManager = _userManager;
            _httpClient = httpClient;
        }

        public class VehicleDataUpdateService : IHostedService, IDisposable
        {
            private Timer _timer;
            private readonly IServiceProvider _serviceProvider;

            // Thông tin Google Sheets
            private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
            private static readonly string ApplicationName = "2024.THIL_TRANS_BaoDuongPhuongTien";
            private static readonly string SpreadsheetId = "1ukknx8QxaxA3ZKf77Hk1dBhMjCvb2jjqVYxc1viEetg";
            private static readonly string SheetName = "DATA_ADSUN"; // Thay thế bằng tên sheet cụ thể của bạn
            private SheetsService _sheetsService;

            public VehicleDataUpdateService(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                // Thiết lập Google Sheets API
                InitializeGoogleSheetsService();

                // Đặt thời gian chạy định kỳ mỗi giờ
                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
                return Task.CompletedTask;
            }

            private async void DoWork(object state)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var vehicleService = scope.ServiceProvider.GetRequiredService<VehicleService>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitofWork>();

                    var vehicleList = await vehicleService.GetVehicleInfoAsync();

                    if (vehicleList != null && vehicleList.Count > 0)
                    {
                        foreach (var vehicle in vehicleList)
                        {
                            var existingVehicle = uow.Adsuns.GetAll(v => v.Plate == vehicle.Plate).FirstOrDefault();

                            if (existingVehicle != null)
                            {
                                // Cập nhật dữ liệu nếu biển số đã tồn tại
                                existingVehicle.GroupId = vehicle.GroupId;
                                existingVehicle.GroupName = vehicle.GroupName;
                                existingVehicle.Lat = vehicle.Lat.ToString();
                                existingVehicle.Lng = vehicle.Lng.ToString();
                                existingVehicle.Speed = vehicle.Speed.ToString();
                                existingVehicle.Km = vehicle.Km.ToString();
                                existingVehicle.Gsm = vehicle.Gsm;
                                existingVehicle.Gps = vehicle.Gps;
                                existingVehicle.Key = vehicle.Key;
                                existingVehicle.Door = vehicle.Door;
                                existingVehicle.Temper = vehicle.Temper;
                                existingVehicle.Temper2 = vehicle.Temper2;
                                existingVehicle.Fuel = vehicle.Fuel.ToString();
                                existingVehicle.DriverName = vehicle.DriverName;
                                existingVehicle.Liciense = vehicle.Liciense;
                                existingVehicle.TimeUpdate = vehicle.TimeUpdate;
                                existingVehicle.Address = vehicle.Address;
                                existingVehicle.InputPower = vehicle.InputPower.ToString();
                                existingVehicle.TripKm = vehicle.TripKm.ToString();
                                existingVehicle.IsStop = vehicle.IsStop;
                                existingVehicle.StopTime = vehicle.StopTime;
                                existingVehicle.StopCounter = vehicle.StopCounter.ToString();
                                existingVehicle.Angle = vehicle.Angle.ToString();
                                existingVehicle.ACOnOff = vehicle.ACOnOff;
                                existingVehicle.IsOverSpeed = vehicle.IsOverSpeed;
                                existingVehicle.OverSpeedCount = vehicle.OverSpeedCount.ToString();
                                existingVehicle.BeginStop = vehicle.BeginStop;
                                existingVehicle.DayDrivingTime = vehicle.DayDrivingTime.ToString();
                                existingVehicle.DrivingTime = vehicle.DrivingTime.ToString();
                                existingVehicle.Over10h = vehicle.Over10h;
                                existingVehicle.Over4h = vehicle.Over4h;
                                existingVehicle.VehicleType = vehicle.VehicleType;
                                existingVehicle.SheeatsOrTons = vehicle.SheeatsOrTons;
                                existingVehicle.UpdatedDate = DateTime.Now;

                                uow.Adsuns.Update(existingVehicle);
                            }
                            else
                            {
                                // Thêm mới nếu biển số chưa tồn tại
                                var LsAdsun = new Adsun
                                {
                                    Id = vehicle.Id,
                                    Plate = vehicle.Plate,
                                    GroupId = vehicle.GroupId,
                                    GroupName = vehicle.GroupName,
                                    Lat = vehicle.Lat.ToString(),
                                    Lng = vehicle.Lng.ToString(),
                                    Speed = vehicle.Speed.ToString(),
                                    Km = vehicle.Km.ToString(),
                                    Gsm = vehicle.Gsm,
                                    Gps = vehicle.Gps,
                                    Key = vehicle.Key,
                                    Door = vehicle.Door,
                                    Temper = vehicle.Temper,
                                    Temper2 = vehicle.Temper2,
                                    Fuel = vehicle.Fuel.ToString(),
                                    DriverName = vehicle.DriverName,
                                    Liciense = vehicle.Liciense,
                                    TimeUpdate = vehicle.TimeUpdate,
                                    Address = vehicle.Address,
                                    InputPower = vehicle.InputPower.ToString(),
                                    TripKm = vehicle.TripKm.ToString(),
                                    IsStop = vehicle.IsStop,
                                    StopTime = vehicle.StopTime,
                                    StopCounter = vehicle.StopCounter.ToString(),
                                    Angle = vehicle.Angle.ToString(),
                                    ACOnOff = vehicle.ACOnOff,
                                    IsOverSpeed = vehicle.IsOverSpeed,
                                    OverSpeedCount = vehicle.OverSpeedCount.ToString(),
                                    BeginStop = vehicle.BeginStop,
                                    DayDrivingTime = vehicle.DayDrivingTime.ToString(),
                                    DrivingTime = vehicle.DrivingTime.ToString(),
                                    Over10h = vehicle.Over10h,
                                    Over4h = vehicle.Over4h,
                                    VehicleType = vehicle.VehicleType,
                                    SheeatsOrTons = vehicle.SheeatsOrTons,
                                    CreatedDate = DateTime.Now,
                                };

                                uow.Adsuns.Add(LsAdsun);

                            }
                        }

                        // Lưu tất cả các thay đổi vào cơ sở dữ liệu
                        uow.Complete();

                        // Cập nhật Google Sheets sau khi cập nhật cơ sở dữ liệu
                        await UpdateGoogleSheet(vehicleList);
                    }
                }
            }

            private void InitializeGoogleSheetsService()
            {
                // Load the credentials.json from file
                GoogleCredential credential;
                using (var stream = new FileStream("Credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(Scopes);
                }

                // Create Google Sheets API service
                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            private async Task<int> GetSheetIdAsync(string spreadsheetId, string sheetName)
            {
                // Lấy thông tin về bảng tính
                var spreadsheet = await _sheetsService.Spreadsheets.Get(spreadsheetId).ExecuteAsync();

                // Tìm sheet có tên phù hợp
                var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);

                // Nếu không tìm thấy sheet với tên cung cấp
                if (sheet == null)
                {
                    throw new Exception($"Sheet with name '{sheetName}' not found.");
                }

                // Trả về sheetId
                return (int)(sheet?.Properties?.SheetId);
            }
            private async Task UpdateGoogleSheet(List<VehicleInfo> vehicleList)
            {
                // Đọc toàn bộ dữ liệu từ Google Sheets
                var range = $"{SheetName}!A2:AJ";  // Đọc tất cả các cột từ dòng A2
                var request = _sheetsService.Spreadsheets.Values.Get(SpreadsheetId, range);
                var response = await request.ExecuteAsync();
                var upDate = DateTime.Now;
                var timeUpdate = string.Format("{0:dd/MM/yyyy HH:mm}", upDate);

                // Nếu không có dữ liệu, khởi tạo danh sách trống
                var existingRows = response.Values ?? new List<IList<object>>();
                var sheetId = await GetSheetIdAsync(SpreadsheetId, SheetName);

                // Tạo danh sách cập nhật và thêm mới
                var rowsToUpdate = new List<(int, VehicleInfo)>(); // (vị trí hàng, dữ liệu vehicle)
                var rowsToAppend = new List<VehicleInfo>();        // danh sách cần thêm mới

                foreach (var vehicle in vehicleList)
                {
                    bool isUpdated = false;

                    // Duyệt qua dữ liệu hiện có trong Google Sheets
                    for (int i = 0; i < existingRows.Count; i++)
                    {
                        var row = existingRows[i];

                        if (row.Count > 0 && row[1].ToString() == vehicle.Plate)
                        {
                            // Cập nhật nếu Plate đã tồn tại
                            rowsToUpdate.Add((i + 2, vehicle)); // i+2 vì dòng Google Sheets bắt đầu từ 1
                            isUpdated = true;
                            break;
                        }
                    }

                    if (!isUpdated)
                    {
                        // Nếu không tìm thấy Plate, thêm vào danh sách append
                        rowsToAppend.Add(vehicle);
                    }
                }

                // Cập nhật những hàng đã tồn tại (sử dụng batch update)
                if (rowsToUpdate.Count > 0)
                {
                    var batchUpdateRequests = new List<Request>();


                    foreach (var (rowIndex, vehicle) in rowsToUpdate)
                    {
                        var updateCells = new List<CellData>
            {

                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Plate.ToString() } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.TimeUpdate } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Km.ToString() } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Speed.ToString() } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.TripKm.ToString() } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Lat.ToString() } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Lng.ToString() } },
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.GroupId}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.GroupName}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Temper}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Temper2}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.VehicleType}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.DriverName}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Liciense}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Address}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.InputPower.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.StopTime}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.StopCounter.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Angle.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.OverSpeedCount.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.BeginStop}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.DayDrivingTime.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.DrivingTime.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Over10h}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Over4h}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.Fuel.ToString()}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = vehicle.SheeatsOrTons}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.Gsm}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.Gps}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.Key}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.Door}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.IsStop}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.ACOnOff}},
                new CellData { UserEnteredValue = new ExtendedValue { BoolValue = vehicle.IsOverSpeed}},
                new CellData { UserEnteredValue = new ExtendedValue { StringValue = timeUpdate,}},
        
                // Thêm các trường khác nếu cần
            };

                        var updateRequest = new Request
                        {
                            UpdateCells = new UpdateCellsRequest
                            {
                                Range = new GridRange
                                {
                                    SheetId = sheetId,
                                    StartRowIndex = rowIndex - 1,  // Bắt đầu từ dòng cần cập nhật (Google Sheets bắt đầu từ 0)
                                    EndRowIndex = rowIndex,
                                    StartColumnIndex = 1,
                                    EndColumnIndex = 1 + updateCells.Count
                                },
                                Rows = new List<RowData> { new RowData { Values = updateCells } },
                                Fields = "userEnteredValue"
                            }
                        };

                        batchUpdateRequests.Add(updateRequest);
                    }

                    var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
                    {
                        Requests = batchUpdateRequests
                    };

                    try
                    {
                        var batchUpdate = _sheetsService.Spreadsheets.BatchUpdate(batchUpdateRequest, SpreadsheetId);
                        var result = await batchUpdate.ExecuteAsync();
                        Console.WriteLine($"Batch updated {rowsToUpdate.Count} rows.");
                    }
                    catch (GoogleApiException ex)
                    {
                        Console.WriteLine($"Google API Error (Batch Update): {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"General Error (Batch Update): {ex.Message}");
                    }
                }

                // Thêm những hàng chưa tồn tại
                if (rowsToAppend.Count > 0)
                {
                    var appendRange = $"{SheetName}!A:AJ";

                    var appendValues = rowsToAppend.Select(vehicle => new List<object>
        {   "",
            vehicle.Plate, vehicle.TimeUpdate, vehicle.Km.ToString(),vehicle.Speed.ToString(),
            vehicle.TripKm.ToString(),vehicle.Lat.ToString(), vehicle.Lng.ToString(),
            vehicle.GroupId,vehicle.GroupName,vehicle.Temper,vehicle.Temper2,vehicle.VehicleType,
            vehicle.DriverName,vehicle.Liciense,vehicle.Address,vehicle.InputPower.ToString(),vehicle.StopTime,
            vehicle.StopCounter.ToString(),vehicle.Angle.ToString(),vehicle.OverSpeedCount.ToString(),vehicle.BeginStop,vehicle.DayDrivingTime.ToString(),
            vehicle.DrivingTime.ToString(),vehicle.Over10h,vehicle.Over4h,vehicle.Fuel.ToString(),vehicle.SheeatsOrTons,vehicle.Gsm,
            vehicle.Gps,vehicle.Key,vehicle.Door,vehicle.IsStop,vehicle.ACOnOff,vehicle.IsOverSpeed,timeUpdate
            // Thêm các trường khác...
        }).Cast<IList<object>>().ToList();

                    var appendValueRange = new ValueRange { Values = appendValues };

                    var appendRequest = _sheetsService.Spreadsheets.Values.Append(appendValueRange, SpreadsheetId, appendRange);
                    appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;

                    try
                    {
                        var result = await appendRequest.ExecuteAsync();
                        Console.WriteLine($"Appended {rowsToAppend.Count} new rows.");
                    }
                    catch (GoogleApiException ex)
                    {
                        Console.WriteLine($"Google API Error (Append): {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"General Error (Append): {ex.Message}");
                    }
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

        // Ví dụ về một route cập nhật dữ liệu

    }
}


//Lấy bảng tính đầu tiên
// private async Task<string> GetFirstSheetNameAsync()
// {
//     var spreadsheet = await _sheetsService.Spreadsheets.Get(SpreadsheetId).ExecuteAsync();
//     var firstSheet = spreadsheet.Sheets.FirstOrDefault();  // Lấy sheet đầu tiên
//     if (firstSheet != null)
//     {
//         return firstSheet.Properties.Title;  // Trả về tên của sheet đầu tiên
//     }
//     return null;
// }

// private async Task UpdateGoogleSheet(List<VehicleInfo> vehicleList)
// {
//     // Lấy tên sheet đầu tiên
//     var firstSheetName = await GetFirstSheetNameAsync();
//     if (string.IsNullOrEmpty(firstSheetName))
//     {
//         Console.WriteLine("No sheet found in the spreadsheet.");
//         return;
//     }

//     // Đọc toàn bộ dữ liệu từ sheet đầu tiên
//     var range = $"{firstSheetName}!A2:Z";  // Đọc tất cả các cột từ dòng A2
//     var request = _sheetsService.Spreadsheets.Values.Get(SpreadsheetId, range);
//     var response = await request.ExecuteAsync();

//     // Phần còn lại của logic cập nhật hoặc thêm mới...
// }