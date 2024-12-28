
using System.Collections.Generic;
using System.Net.Http;
using System;

namespace ERP.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        public static int GetNumberOfDaysInMonth(int year, int month)
        {
            // Nếu tháng là 2 (tháng 2), kiểm tra xem năm có phải là năm nhuận không.
            if (month == 2)
            {
                if (IsLeapYear(year))
                    return 29;
                else
                    return 28;
            }

            // Nếu tháng là 4, 6, 9, 11 (tháng có 30 ngày).
            if (month == 4 || month == 6 || month == 9 || month == 11)
                return 30;

            // Các tháng còn lại có 31 ngày.
            return 31;
        }

        private static bool IsLeapYear(int year)
        {
            // Năm nhuận là năm chia hết cho 4, nhưng không chia hết cho 100,
            // trừ trường hợp năm chia hết cho 400 thì vẫn là năm nhuận.
            return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
        }
    
        public string Issuer { get; set; }
        public string SecretQrCode { get; set; }
    }
}