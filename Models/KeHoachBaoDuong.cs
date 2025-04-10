using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class KeHoachBaoDuong : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ForeignKey("BaoDuong")]
        public Guid? BaoDuong_Id { get; set; }
        public DM_Model BaoDuong { get; set; }

        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }

        [ForeignKey("ThietBi")]
        public Guid? ThietBi_Id { get; set; }
        public DS_ThietBi ThietBi { get; set; }
        public string Note { get; set; }
        public string IsQuaHan { get; set; }
        public DateTime? NgayTao { get; set; }
        public Guid? DiaDiem_Id { get; set; }

    }
}
