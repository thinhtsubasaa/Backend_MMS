using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class LichSuKiemTraHangNgay : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }
        public string GhiChu { get; set; }
        public string TinhTrang { get; set; }
        public string HinhAnh { get; set; }
        public int SoKM { get; set; }
        public string NguoiKiemTra { get; set; }

    }
}
