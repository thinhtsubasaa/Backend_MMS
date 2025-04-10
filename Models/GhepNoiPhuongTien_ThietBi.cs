using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class GhepNoiPhuongTien_ThietBi : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public Guid? DonVi_Id { get; set; }

        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }

        [ForeignKey("ThietBi")]
        public Guid? ThietBi_Id { get; set; }
        public DS_ThietBi ThietBi { get; set; }

        [ForeignKey("ThietBi2")]
        public Guid? ThietBi2_Id { get; set; }
        public DS_ThietBi ThietBi2 { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TenThietBi { get; set; }
    }
}
