using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ERP.Helpers
{
    public class DownloadImage
    {
        // public HttpClient Client { get; set; }

        public DownloadImage()
        {

        }

        public string ImageHRM(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            string stringPath = "Uploads/HinhAnhNV";
            string fullPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, stringPath);
            var localImagePath = Path.Combine(fullPath, fileName);
            string baseApiUrl = Commons.ApiUrl;
            string urlPath = Path.Combine(stringPath, fileName).Replace("\\", "/");
            if (System.IO.File.Exists(localImagePath))
            {
                return baseApiUrl + urlPath;
            }

            return imageUrl;
        }
    }



    public class MyTypedClient
    {
        public HttpClient Client { get; set; }
        public MyTypedClient(HttpClient client)
        {
            string apiKey = "THACO2017";
            client.BaseAddress = new Uri("https://portalgroupapi.thacochulai.vn");
            client.DefaultRequestHeaders.Add("Authorization", "APIKEY " + apiKey);
            client.DefaultRequestHeaders.Add("APIKEY", apiKey);
            this.Client = client;
        }

        public string AnhNhanVien(string MaNhanVien)
        {
            string url = "";
            var response = this.Client.GetAsync("/api/KeySecure/AnhNhanVien?MaNhanVien=" + MaNhanVien).Result;
            if (response.IsSuccessStatusCode)
            {
                url = response.Content.ReadAsStringAsync().Result;
            }
            return url;
        }
        public class NhanVien1
        {
            public string maNhanVien { get; set; }
            public string tenNhanVien { get; set; }
            public string hinhAnh_Url { get; set; }
            public string tenPhongBan { get; set; }
            public string chucDanh { get; set; }
            public string chucVu { get; set; }
            public string trangThai { get; set; }
        }
        public class NhanVienHRMModel
        {
            public string MaNhanVien { get; set; }
            public string TenNhanVien { get; set; }
            public string HinhAnh_Url { get; set; }
            public string TenPhongBan { get; set; }
            public string ChuoiPhongBan { get; set; }
            public string ChucDanh { get; set; }
            public string ChucVu { get; set; }
            public string TrangThai { get; set; }
        }
        public NhanVienHRMModel ThongTinNhanVien(string MaNhanVien)
        {
            var data = new NhanVienHRMModel();
            var response = this.Client.GetAsync("/api/KeySecure/ThongTinNhanVien?MaNhanVien=" + MaNhanVien).Result;
            if (response.IsSuccessStatusCode)
            {
                var res = response.Content.ReadAsStringAsync().Result;
                data = JsonConvert.DeserializeObject<NhanVienHRMModel>(res);
            }
            return data;
        }
    }
    public class VehicleInfo
    {
        public string Id { get; set; }
        public string Plate { get; set; }

        public string GroupId { get; set; }

        public string GroupName { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }

        public string Speed { get; set; }

        public string Km { get; set; }
        public bool Gsm { get; set; }
        public bool Gps { get; set; }
        public bool Key { get; set; }
        public bool Door { get; set; }

        public string Temper { get; set; }
        public string Temper2 { get; set; }
        public string Fuel { get; set; }
        public string DriverName { get; set; }
        public string Liciense { get; set; }
        public string TimeUpdate { get; set; }
        public string Address { get; set; }
        public string InputPower { get; set; }
        public string TripKm { get; set; }
        public bool IsStop { get; set; }
        public string StopTime { get; set; }
        public string StopCounter { get; set; }
        public string Angle { get; set; }
        public bool ACOnOff { get; set; }
        public bool IsOverSpeed { get; set; }
        public string OverSpeedCount { get; set; }
        public string BeginStop { get; set; }
        public string DayDrivingTime { get; set; }
        public string DrivingTime { get; set; }
        public string Over10h { get; set; }
        public string Over4h { get; set; }
        public string VehicleType { get; set; }
        public string SheeatsOrTons { get; set; }

        // Thêm các thuộc tính khác nếu cần
    }
    public class ApiResponse
    {
        public List<VehicleInfo> Data { get; set; }
    }

    public class VehicleService
    {
        private readonly HttpClient _httpClient;

        public VehicleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<VehicleInfo>> GetVehicleInfoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://shareapi.adsun.vn/Vehicle/GpsInfos?pageIds=4279&username=cty4279&pwd=123456");
                response.EnsureSuccessStatusCode();

                var res = await response.Content.ReadAsStringAsync();
                var vehicleList = JsonConvert.DeserializeObject<ApiResponse>(res)?.Data;
                return vehicleList ?? new List<VehicleInfo>();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Request error: {e.Message}");
                return new List<VehicleInfo>();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
                return new List<VehicleInfo>();
            }
        }

        public async Task<VehicleInfo> GetVehicleByPlateAsync(string plate)
        {
            var vehicleList = await GetVehicleInfoAsync();
            var vehicle = vehicleList.FirstOrDefault(v => v.Plate.Equals(plate, StringComparison.OrdinalIgnoreCase));
            return vehicle;
        }
    }


}
