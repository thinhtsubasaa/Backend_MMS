using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class LichSuBaoDuong_ChiTiet : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("LichSuBaoDuong")]
        public Guid? LichSuBaoDuong_Id { get; set; }
        public LichSuBaoDuong LichSuBaoDuong { get; set; }

        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }

        [ForeignKey("HangMuc")]
        public Guid? HangMuc_Id { get; set; }
        public DM_HangMuc HangMuc { get; set; }
        public string GhiChu { get; set; }
        public string HinhAnh { get; set; }
        public int ChiPhi { get; set; }

    }
}
