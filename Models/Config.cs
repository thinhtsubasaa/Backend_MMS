using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Drawing.Printing;
using System.Linq;

namespace ERP.Models
{
    public class Config : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Phân trang chỉ được nhập số")]
        [Range(20, 500, ErrorMessage = "Phân trang phải nằm trong khoảng từ 20 đến 500")]
        public int PageSize { get; set; }
    }
}
