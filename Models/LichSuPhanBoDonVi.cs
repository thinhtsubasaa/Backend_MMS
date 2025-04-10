using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class LichSuPhanBoDonVi : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string BienSo { get; set; }
        public Guid? DonVi_Id { get; set; }
        public string TenDonVi { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public Guid? BoPhan_Id { get; set; }
        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }

    }
}
