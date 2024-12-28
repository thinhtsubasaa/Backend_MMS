using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using ERP.Helpers;
// using NETCORE3.Infrastructure;
// using NETCORE3.Models;
// using static NETCORE3.Data.MyDbContext;

using Microsoft.Extensions.Configuration;


namespace ERP
{
    public class Commons
    {
        private static IConfiguration AppSetting { get; }
        static Commons()
        {
            AppSetting = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json")
                      .Build();
        }
        public static string UploadBase64(string webRootPath, string File_Base64, string File_Name)
        {
            //Xử lý file base 64 lưu trữ
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = DateTime.Now;
            string fileName = (long)timeSpan.TotalSeconds + "_" + TiengVietKhongDau(File_Name);
            byte[] fileBytes = Convert.FromBase64String(File_Base64.Split(',')[1]);
            var buffer = Convert.FromBase64String(Convert.ToBase64String(fileBytes));
            // Convert byte[] to file type
            string path = "Uploads/" + dt.Year + "/" + dt.Month + "/" + dt.Day;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            }
            if (!Directory.Exists(webRootPath))
            {
                Directory.CreateDirectory(webRootPath);
            }
            string fullPath = Path.Combine(webRootPath, fileName);
            System.IO.FileStream f = System.IO.File.Create(fullPath);
            f.Close();
            System.IO.File.WriteAllBytes(fullPath, buffer);
            return path + "/" + fileName;
        }
        public static string Upload(string webRootPath, IFormFile file)
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = DateTime.Now;
            // Rename file
            string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
            string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            string path = "Uploads/" + dt.Year + "/" + dt.Month + "/" + dt.Day;
            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), path);
            }
            if (!Directory.Exists(webRootPath))
            {
                Directory.CreateDirectory(webRootPath);
            }
            string fullPath = Path.Combine(webRootPath, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return path + "/" + fileName;
        }
        public static int SoNgay(DateTime Ngay)
        {
            return (DateTime.Now - Ngay).Days;
        }
        public static string ApiUrl = "https://apiwms.thilogi.vn/";
        public static string ApiUrlMMS = "https://10.17.41.233:5001/";
        public static string NonUnicode(string text)
        {
            string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ",};
            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y",};
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }

        public static object LockObjectState = new object();
        public static string TiengVietKhongDau(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, string.Empty).Replace("đ", "d").Replace("Đ", "D").Replace(' ', '_').ToLower();
        }
        public static float ConvertFloat(string number)
        {
            float num = 0;
            float.TryParse(number, out num);
            return num;
        }
        public static float TinhTrungBinh(params float[] array)
        {
            return array.Average();
        }
        public static string ConvertObjectToJson(object ob)
        {
            return JsonConvert.SerializeObject(ob);
        }
        /*    public static Image HinhAnhUrl(string url)
            {
              var base_url = "http://demo1api.thacoindustries.vn/" + url;
                    WebClient wc = new WebClient();
                    byte[] bytes = wc.DownloadData(base_url);
              MemoryStream ms = new MemoryStream(bytes);
              System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
              return img;
            }*/


        //firebase
        public class FirebaseNotificationSender
        {
            private readonly FirebaseApp FirebaseAppInstance;
            private readonly string CredentialsFilePath;

            /*            public FirebaseNotificationSender()
                        {
                            FirebaseAppInstance = InitializeFirebaseAppInstance();
                        }

                        private FirebaseApp InitializeFirebaseAppInstance()
                        {
                            var credential = GoogleCredential.FromFile("D:/Thaco_Dev_QLTB/firebase-admin/qlcntt-bf148-firebase-adminsdk-37kuv-c9f2e3d983.json");
                            return FirebaseApp.Create(new AppOptions
                            {
                                Credential = credential,
                                ProjectId = "qlcntt-bf148"
                            });
                        }*/
            public FirebaseNotificationSender()
            {
                CredentialsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-admin", "qlcntt-bf148-firebase-adminsdk-37kuv-c9f2e3d983.json");
                FirebaseAppInstance = InitializeFirebaseAppInstance();
            }

            private FirebaseApp InitializeFirebaseAppInstance()
            {
                var credential = GoogleCredential.FromFile(CredentialsFilePath);
                return FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = "qlcntt-bf148"
                });
            }

            public void SendNotification(string title, string body, string registrationToken)
            {


                try
                {
                    var messaging = FirebaseMessaging.GetMessaging(FirebaseAppInstance);

                    var message = new Message
                    {
                        Token = registrationToken,
                        Notification = new Notification
                        {
                            Title = title,
                            Body = body
                        }
                    };
                    messaging.SendAsync(message).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions here, log, or throw.
                }
            }
            public void SendNotificationWithCustomData(string registrationToken, Dictionary<string, string> customData, string title, string body, string link)
            {
                try
                {
                    var messaging = FirebaseMessaging.GetMessaging(FirebaseAppInstance);

                    var message = new Message
                    {
                        Token = registrationToken,
                        Data = customData
                    };

                    var notification = new Notification
                    {
                        Title = title,
                        Body = body,
                        //ImageUrl = "https://qlcntt.thacoindustries.com/static/media/logo-industries.04373ce1.jpg"
                    };

                    var webpushOptions = new WebpushFcmOptions
                    {
                        Link = link
                    };

                    message.Notification = notification;
                    message.Webpush = new WebpushConfig
                    {
                        FcmOptions = new WebpushFcmOptions
                        {
                            Link = link
                        }
                    };
                    messaging.SendAsync(message).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    // Handle any exceptions here, log, or throw.
                }
            }

            public void HandleNotificationClick(string link)
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = link,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu cần
                }
            }

        }

        private static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[128];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static string HashPassword(string password)
        {
            byte[] salt = GenerateRandomSalt();

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] hashBytes = argon2.GetBytes(128);

                string hashedPassword = Convert.ToBase64String(hashBytes);
                string saltString = Convert.ToBase64String(salt);

                return $"{saltString}${hashedPassword}";
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            string[] parts = storedHash.Split('$');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] hashBytes = Convert.FromBase64String(parts[1]);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(enteredPassword)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                byte[] newHashBytes = argon2.GetBytes(128);

                return hashBytes.SequenceEqual(newHashBytes);
            }
        }

        //kiểm tra có chữ hoa chữ, có số, có ký tự đặt biệt
        public class CustomPasswordValidationAttribute : ValidationAttribute
        {
            public override bool IsValid(object value)
            {
                string password = value as string;

                if (string.IsNullOrEmpty(password))
                    return false;

                bool hasUpperCase = password.Any(char.IsUpper);
                bool hasSpecialCharacter = password.Any(c => !char.IsLetterOrDigit(c));
                bool hasDigit = password.Any(char.IsDigit);

                return hasUpperCase && hasSpecialCharacter && hasDigit;
            }
        }

        public static object ConvertValueByType(string value, string type, bool allow_empty_value)
        {
            type = type.ToLower();

            if (allow_empty_value && string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (type == "string")
            {
                return value;
            }
            else if (type == "number")
            {
                if (int.TryParse(value, out int intValue))
                {
                    return intValue;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (type == "datetime")
            {
                if (DateTime.TryParse(value, out DateTime dateTimeValue))
                {
                    return dateTimeValue;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (type == "boolean")
            {
                if (bool.TryParse(value, out bool boolValue))
                {
                    return boolValue;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return null;
        }
        public static string EnCode(string plainText)
        {
            var appSettingsSection = AppSetting.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.SecretQrCode);
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }
        public static string DeCode(string cipherText)
        {
            var appSettingsSection = AppSetting.GetSection("AppSettings");
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.SecretQrCode);
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }


        /* if (data.UserXuLy_Id == Guid.Parse(User.Identity.Name))
                            {
                                _hubContext.Clients.All.SendAsync("ReceiveMessage", data.UserLap_Id, phanhoi.NoiDungPhanHoi);
                            }
                            else
                            {
                                _hubContext.Clients.All.SendAsync("ReceiveMessage", data.UserXuLy_Id, phanhoi.NoiDungPhanHoi);
                            }*/
        /*        public class ChatHub : Hub
                {
                    public async Task SendMessage(string user, string message)
                    {
                        await Clients.All.SendAsync("ReceiveMessage", user, message);
                    }
                }*/
    }
}
