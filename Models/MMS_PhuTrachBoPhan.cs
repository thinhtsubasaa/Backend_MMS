using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class MMS_PhuTrachBoPhan : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? User_Id { get; set; }
        public Guid? User2_Id { get; set; }
        public bool IsFull { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }
        public string NhanVien { get; set; }
        public string HinhAnh_NhanVien { get; set; }
        public string MaNhanVien { get; set; }
    }
}
