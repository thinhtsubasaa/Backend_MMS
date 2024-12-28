using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class CBNV_DieuChuyen
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ForeignKey("DieuChuyenNhanVien")]
        public Guid? DieuChuyenNhanVien_Id { get; set; }
        public DieuChuyenNhanVien DieuChuyenNhanVien { get; set; }
        [ForeignKey("User")]
        public Guid? User_Id { get; set; }
        public ApplicationUser User { get; set; }
        public Guid? DonViTraLuongNew_Id { get; set; }
        public DonVi DonViTraLuongNew { get; set; }
        public Guid? DonViTraLuong_Id { get; set; }
        public DonVi DonViTraLuong { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
}
