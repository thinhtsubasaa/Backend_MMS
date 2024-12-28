using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ERP.Infrastructure;
using ERP.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Identity;
using static ERP.Data.MyDbContext;
using OfficeOpenXml;
using System.Globalization;
using QRCoder;
// using System.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using SixLabors.ImageSharp.Formats.Jpeg;
using ERP.Helpers;
using static ERP.Helpers.MyTypedClient;
using System.Text.RegularExpressions;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public UploadController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }
        [HttpPost]
        public ActionResult Upload(IFormFile file, string stringPath)
        {
            lock (Commons.LockObjectState)
            {
                var timeSpan = DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0);
                DateTime dt = DateTime.Now;
                string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
                string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                if (stringPath != null)
                {
                    try
                    {
                        string fullPathToDelete = Directory.GetCurrentDirectory() + "/" + stringPath;
                        if (System.IO.File.Exists(fullPathToDelete))
                        {
                            System.IO.File.Delete(fullPathToDelete);
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status409Conflict, "File không tồn tại!");
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Đã xảy ra lỗi: " + ex.Message);
                    }
                }
                stringPath = "Uploads/File/" + dt.Year + "/" + dt.Month;
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), stringPath, fileName);
                string directoryPath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Ok(new FileModel { Path = stringPath + "/" + fileName, FileName = file.FileName });
            }
        }

        [HttpPost("UpLoadAvatar")]
        public ActionResult UploadAvatar(IFormFile file, string stringPath)
        {
            lock (Commons.LockObjectState)
            {
                var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var timeSpan = DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0);
                DateTime dt = DateTime.Now;
                // string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
                // string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                string baseFileName = $"{exit.MaNhanVien}_{exit.FullName}";
                string fileExt = Path.GetExtension(file.FileName).ToLower();
                if (stringPath != null)
                {
                    try
                    {
                        string fullPathToDelete = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/" + stringPath;
                        if (System.IO.File.Exists(fullPathToDelete))
                        {
                            System.IO.File.Delete(fullPathToDelete);
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status409Conflict, "File không tồn tại!");
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Đã xảy ra lỗi: " + ex.Message);
                    }
                }
                string baseApiUrl = Commons.ApiUrl;
                // string baseApiUrl = "https://10.17.41.181/";
                stringPath = "Uploads/Avatar/" + exit.FullName;
                // string fullPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath, fileName);
                // string directoryPath = Path.GetDirectoryName(fullPath);
                string directoryPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                int count = 1;
                string fileName = $"{baseFileName}_{count}{fileExt}";
                string fullPath = Path.Combine(directoryPath, fileName);

                // Lặp cho đến khi tìm được tên file chưa tồn tại
                while (System.IO.File.Exists(fullPath))
                {
                    count++;
                    fileName = $"{baseFileName}_{count}{fileExt}";
                    fullPath = Path.Combine(directoryPath, fileName);
                }
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Ok(new FileModel { Path = baseApiUrl + stringPath + "/" + fileName, FileName = file.FileName });
            }
        }


        [HttpPost("UpLoadLichSu")]
        public ActionResult UploadLichSu(IFormFile file, string stringPath)
        {
            lock (Commons.LockObjectState)
            {
                var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var timeSpan = DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0);
                DateTime dt = DateTime.Now;
                // string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
                // string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                // // string fileName = $"{exit.MaNhanVien}_{exit.FullName}_{dt.Month}_{dt.Year}.jpg";
                string baseFileName = $"{exit.MaNhanVien}_{exit.FullName}_{dt.Month}_{dt.Year}";
                string fileExt = Path.GetExtension(file.FileName).ToLower();

                if (stringPath != null)
                {
                    try
                    {
                        string fullPathToDelete = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/" + stringPath;
                        if (System.IO.File.Exists(fullPathToDelete))
                        {
                            System.IO.File.Delete(fullPathToDelete);
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status409Conflict, "File không tồn tại!");
                        }
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status409Conflict, "Đã xảy ra lỗi: " + ex.Message);
                    }
                }

                string baseApiUrl = Commons.ApiUrl;
                // string baseApiUrl = "https://10.17.41.181/";
                stringPath = "Uploads/FileDinhKem/" + dt.Year + "/" + dt.Month;
                // string fullPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath, fileName);
                // string directoryPath = Path.GetDirectoryName(fullPath);
                // if (!Directory.Exists(directoryPath))
                // {
                //     Directory.CreateDirectory(directoryPath);
                // }
                string directoryPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Đếm số thứ tự để tạo tên file duy nhất
                int count = 1;
                string fileName = $"{baseFileName}_{count}{fileExt}";
                string fullPath = Path.Combine(directoryPath, fileName);

                // Lặp cho đến khi tìm được tên file chưa tồn tại
                while (System.IO.File.Exists(fullPath))
                {
                    count++;
                    fileName = $"{baseFileName}_{count}{fileExt}";
                    fullPath = Path.Combine(directoryPath, fileName);
                }
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return Ok(new FileModel { Path = baseApiUrl + stringPath + "/" + fileName, FileName = file.FileName });
            }
        }

        [HttpPost("Read_Excel")]
        public ActionResult Read_Excel(IFormFile file)
        {
            lock (Commons.LockObjectState)
            {
                var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
                DateTime dt = DateTime.Now;
                // Rename file
                string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
                string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                string[] supportedTypes = new[] { "xls", "xlsx" };
                if (supportedTypes.Contains(fileExt))
                {
                    string webRootPath = environment.WebRootPath;
                    if (string.IsNullOrWhiteSpace(webRootPath))
                    {
                        webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                    }
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
                        var list_datas = new List<ImportDSTheNV>();
                        var list_giaoxe = new ImportDSTheNV();
                        //                          object NgayDongCont = worksheet.Cells[5, 13].Value;
                        // list_dongcont.NgayDongCont = (DateTime)NgayDongCont;
                        for (int i = 2; i <= rowCount; i++)
                        {
                            if (worksheet.Cells[i, 2].Value == null)
                            {
                                // Nếu không có dữ liệu, dừng vòng lặp
                                break;
                            }
                            object Msnv = worksheet.Cells[i, 1].Value;
                            object Hovaten = worksheet.Cells[i, 2].Value;
                            object PhongBan = worksheet.Cells[i, 3].Value;
                            object HinhAnh = worksheet.Cells[i, 4].Value;
                            object QRCode = worksheet.Cells[i, 5].Value;


                            DateTime baseDate = new DateTime(1900, 1, 1);
                            int excelDate = 45329;
                            var info = new ImportDSTheNV();

                            info.Msnv = Msnv?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                            info.Hovaten = Hovaten?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                            info.PhongBan = PhongBan?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                            info.HinhAnh = HinhAnh?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                            info.QrCode = QRCode?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                            //Kiểm tra lỗi dữ liệu đầu vào
                            var lst_Lois = new List<string>();
                            info.lst_Lois = lst_Lois;
                            list_datas.Add(info);
                        }

                        list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();

                        list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();
                        return Ok(new { errorCount = list_datas.Where(x => x.IsLoi).Count(), rowCount = list_datas.Count(), list_datas });

                    }
                }
                else
                    return BadRequest("Định dạng tệp tin không cho phép");
            }
        }
        // private byte[] GenerateQRCode(string maNhanVien)
        // {
        //     QRCodeGenerator qrGenerator = new QRCodeGenerator();
        //     QRCodeData qrCodeData = qrGenerator.CreateQrCode(maNhanVien, QRCodeGenerator.ECCLevel.Q);
        //     PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        //     byte[] qrCodeImageBytes = qrCode.GetGraphic(30);
        //     // Kích thước pixel của QR code

        //     return qrCodeImageBytes;
        // }

        private byte[] GenerateQRCode(string maNhanVien)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(maNhanVien, QRCodeGenerator.ECCLevel.Q);

            // Tính toán kích thước cho mỗi module để đạt được 794x794 pixels
            int pixelsPerModule = 794 / qrCodeData.ModuleMatrix.Count;

            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            // Tạo hình ảnh QR code với kích thước 794x794 pixels
            byte[] qrCodeImageBytes = qrCode.GetGraphic(pixelsPerModule);

            return qrCodeImageBytes;
        }

        [HttpPost("Save_Import_QRCode")]
        public ActionResult Post_SaveImport(List<ImportDSTheNV> DH)
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
                    // Tạo QR code cho mỗi mã nhân viên (Msnv)
                    var qrCodeImageBytes = GenerateQRCode(item.Msnv);

                    // Đường dẫn thư mục lưu QR code
                    string stringPath = "Uploads/QrCode/";
                    string directoryPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Tạo tên file dựa trên mã nhân viên và số thứ tự
                    string baseFileName = $"{item.Msnv}_QR";
                    string fileExt = ".png";  // Định dạng file QR code (PNG)
                    int count = 1;
                    string fileName = $"{baseFileName}{fileExt}";
                    string fullPath = Path.Combine(directoryPath, fileName);

                    // Lặp cho đến khi tìm được tên file chưa tồn tại
                    while (System.IO.File.Exists(fullPath))
                    {
                        count++;
                        fileName = $"{baseFileName}{fileExt}";
                        fullPath = Path.Combine(directoryPath, fileName);
                    }

                    // Lưu file QR code dưới dạng mảng byte
                    System.IO.File.WriteAllBytes(fullPath, qrCodeImageBytes);
                }

                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        [HttpPost("Save_Import_HinhAnh")]
        public async Task<ActionResult> Post_SaveImport_HinhAnh(List<ImportDSTheNV> DH)
        {
            await semaphore.WaitAsync(); // Đợi đến khi có quyền truy cập
            bool hasTimeout = false;

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    MyTypedClient nhanvienService = new MyTypedClient(httpClient);

                    foreach (var item in DH)
                    {
                        // Lấy thông tin nhân viên từ mã nhân viên
                        NhanVienHRMModel app_user = nhanvienService.ThongTinNhanVien(item.Msnv);

                        // Bỏ qua nếu không tìm thấy nhân viên hoặc không có URL ảnh
                        if (app_user == null || string.IsNullOrEmpty(app_user.HinhAnh_Url))
                        {
                            continue;
                        }

                        try
                        {
                            // Bước 1: Tải ảnh trực tiếp từ URL
                            string fullPath = await DownloadAndCompressImage(app_user.HinhAnh_Url, item.Msnv);
                            if (string.IsNullOrEmpty(fullPath))
                            {
                                Console.WriteLine($"Lỗi khi tải ảnh từ URL {app_user.HinhAnh_Url}");
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            hasTimeout = true;
                            Console.WriteLine($"Thời gian chờ đã vượt quá cho URL {app_user.HinhAnh_Url}");
                            continue;
                        }
                        catch (AggregateException ex) when (ex.InnerExceptions.Any(e => e is TaskCanceledException))
                        {
                            hasTimeout = true;
                            Console.WriteLine("Timeout xảy ra do AggregateException.");
                            continue;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi xảy ra: {ex.Message}");
                            continue;
                        }
                    }
                }

                uow.Complete();

                if (hasTimeout)
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Có yêu cầu vượt quá thời gian chờ.");
                }

                return StatusCode(StatusCodes.Status200OK);
            }
            finally
            {
                semaphore.Release();
            }
        }

        // Hàm tải và nén ảnh
        private async Task<string> DownloadAndCompressImage(string imageUrl, string msnv)
        {
            // Đường dẫn thư mục lưu ảnh
            string stringPath = "Uploads/HinhAnhNV1/";
            string directoryPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath);

            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Tạo tên file dựa trên mã nhân viên (msnv)
            string fileName = $"{msnv}_Hinh.jpg";
            string fullPath = Path.Combine(directoryPath, fileName);

            // Nếu file đã tồn tại, bỏ qua tải lại
            if (System.IO.File.Exists(fullPath))
            {
                Console.WriteLine($"File {fileName} đã tồn tại, không tải lại.");
                return fullPath;
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();

                    // Nén ảnh
                    using (var imageStream = new MemoryStream(imageBytes))
                    using (var image = SixLabors.ImageSharp.Image.Load(imageStream))
                    {
                        // Thiết lập encoder với chất lượng nén 70%
                        var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                        {
                            Quality = 70
                        };

                        // Kiểm tra và thay đổi kích thước nếu ảnh quá lớn
                        if (image.Width > 200 || image.Height > 200)
                        {
                            image.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Size = new SixLabors.ImageSharp.Size(400, 600),
                                Mode = ResizeMode.Max
                            }));
                            image.Save(fullPath, encoder);
                        }

                        // Lưu ảnh nén vào file
                        // System.IO.File.WriteAllBytes(fullPath, compressedImageStream.ToArray());
                        Console.WriteLine($"Ảnh {fileName} đã được nén và lưu.");
                    }

                    return fullPath;
                }
            }

            return null; // Nếu lỗi, trả về null
        }



        [HttpGet("proxy")]
        public async Task<IActionResult> GetImage([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return StatusCode(StatusCodes.Status404NotFound, "Không tìm thấy url");
            }

            using (var httpClient = new HttpClient())

                try
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var contentType = response.Content.Headers.ContentType?.ToString();
                    var data = await response.Content.ReadAsByteArrayAsync();

                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        return File(data, contentType);
                    }

                    return StatusCode(StatusCodes.Status404NotFound, "Có yêu cầu vượt quá thời gian chờ.");
                }

                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Lỗi khi truy cập URL: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Chi tiết lỗi: {ex.InnerException.Message}");
                    }

                    // Trả về phản hồi lỗi kèm theo chi tiết lỗi
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi truy cập URL: {ex.Message}");
                }
        }


        [HttpPost("Save_Import_HinhAnh_All")]
        public async Task<ActionResult> Post_SaveImport_HinhAnh()
        {
            await semaphore.WaitAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (var httpClient = new HttpClient())
                {
                    MyTypedClient nhanvienService = new MyTypedClient(httpClient);
                    var maNhanVienList = userManager.Users
                        .Where(x => !x.IsDeleted)
                        .Select(x => x.MaNhanVien)
                        .ToList();

                    foreach (var item in maNhanVienList)
                    {
                        NhanVienHRMModel app_user = nhanvienService.ThongTinNhanVien(item);

                        if (app_user == null || string.IsNullOrEmpty(app_user.HinhAnh_Url))
                        {
                            continue;
                        }

                        // Đường dẫn thư mục lưu ảnh nhân viên
                        string stringPath = "Uploads/HinhAnhNV/";
                        string directoryPath = Path.Combine(
                            Directory.GetParent(Directory.GetCurrentDirectory()).FullName,
                            stringPath
                        );

                        // Tạo thư mục nếu chưa tồn tại
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Lấy tên file từ URL hiện tại
                        Uri uri = new Uri(app_user.HinhAnh_Url);
                        string fileName = Path.GetFileName(uri.LocalPath);
                        string fullPath = Path.Combine(directoryPath, fileName);

                        // Kiểm tra nếu file đã tồn tại
                        if (System.IO.File.Exists(fullPath))
                        {
                            Console.WriteLine($"File {fileName} đã tồn tại, không tải lại.");
                            continue;
                        }

                        try
                        {
                            // Tải ảnh từ URL và lưu trực tiếp mà không nén
                            byte[] imageBytes = await httpClient.GetByteArrayAsync(app_user.HinhAnh_Url);
                            await System.IO.File.WriteAllBytesAsync(fullPath, imageBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Lỗi khi tải ảnh từ URL {app_user.HinhAnh_Url}: {ex.Message}");
                            continue;
                        }
                    }
                }

                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
            finally
            {
                semaphore.Release();
            }
        }


        [HttpPost("Save_ThongTinNhanVien")]
        public async Task<ActionResult> Save_ThongTinNhanVien(List<ImportDSTheNV> DH)
        {
            await semaphore.WaitAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using (var httpClient = new HttpClient())
                {
                    MyTypedClient nhanvienService = new MyTypedClient(httpClient);
                    var maNhanVienList = userManager.Users
                        .Where(x => !x.IsDeleted)
                        .Select(x => new { x.MaNhanVien, x.FullName })
                        .ToList();

                    foreach (var item in DH)
                    {
                        // NhanVienHRMModel app_user = nhanvienService.ThongTinNhanVien(item);

                        // if (app_user == null || string.IsNullOrEmpty(app_user.HinhAnh_Url))
                        // {
                        //     continue;
                        // }

                        var user = await userManager.Users.FirstOrDefaultAsync(u => u.MaNhanVien == item.Msnv);

                        if (user == null)
                        {
                            user = new ApplicationUser();
                            user.Id = Guid.NewGuid();
                            // Cập nhật dữ liệu nếu nhân viên đã tồn tại
                            user.CreatedDate = DateTime.Now;
                            user.MaNhanVien = item.Msnv;
                            user.FullName = item.Hovaten;

                            user.UserName = item.Msnv;
                            var result = await userManager.CreateAsync(user);
                            if (!result.Succeeded)
                            {
                                foreach (var error in result.Errors)
                                {
                                    Console.WriteLine($"Error: {error.Description}");
                                }
                                continue;
                            }
                        }


                    }
                }

                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
            finally
            {
                semaphore.Release();
            }
        }

        [HttpPost("Multi")]
        public ActionResult Multi(List<IFormFile> lstFiles)
        {
            lock (Commons.LockObjectState)
            {
                List<FileModel> lst = new List<FileModel>();
                foreach (var file in lstFiles)
                {
                    var timeSpan = (DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0));
                    DateTime dt = DateTime.Now;
                    string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
                    string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                    string stringPath = "Uploads/File/" + dt.Year + "/" + dt.Month;
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), stringPath, fileName);
                    string directoryPath = Path.GetDirectoryName(fullPath);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    lst.Add(new FileModel { Path = "/" + stringPath + "/" + fileName, FileName = file.FileName });
                }
                return Ok(lst);
            }
        }
        [HttpPost("Multi/Image")]
        public ActionResult MultiImage(List<IFormFile> lstFiles)
        {
            lock (Commons.LockObjectState)
            {
                List<FileModel> lst = new List<FileModel>();
                string[] supportedTypes = new[] { "png", "jpg", "jpeg" };
                foreach (var file in lstFiles)
                {
                    var timeSpan = (DateTime.UtcNow - new DateTime(2023, 1, 1, 0, 0, 0));
                    DateTime dt = DateTime.Now;
                    string fileName = (long)timeSpan.TotalSeconds + "_Img-" + Commons.TiengVietKhongDau(file.FileName);
                    string compressedFileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
                    string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
                    if (supportedTypes.Contains(fileExt))
                    {
                        string stringPath = "Uploads/Image/" + dt.Year + "/" + dt.Month;
                        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), stringPath, fileName);
                        string directoryPath = Path.GetDirectoryName(fullPath);
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            file.CopyTo(stream);
                        }
                        string compressedFullPath = Path.Combine(Directory.GetCurrentDirectory(), stringPath, compressedFileName);
                        using (Image image = Image.Load(fullPath))
                        {
                            var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                            {
                                Quality = 80
                            };
                            if (image.Width > 800 || image.Height > 600)
                                image.Mutate(x => x.Resize(new ResizeOptions
                                {
                                    Size = new Size(800, 600),
                                    Mode = ResizeMode.Max
                                }));
                            image.Save(compressedFullPath, encoder);
                        }
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        lst.Add(new FileModel { Path = stringPath + "/" + compressedFileName, FileName = file.FileName });
                    }
                    else
                    {
                        return BadRequest("Chỉ cho phép ảnh với định dạng: png, jpg, jpeg!");
                    }
                }
                return Ok(lst);
            }
        }
    }
}