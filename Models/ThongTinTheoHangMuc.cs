using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class ThongTinTheoHangMuc : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }

        [ForeignKey("HangMuc")]
        public Guid? HangMuc_Id { get; set; }
        public DM_HangMuc HangMuc { get; set; }

        public int GiaTriBaoDuong { get; set; }
        public int TongChiPhi { get; set; }
        public int TongChiPhi_TD { get; set; }

    }
}
