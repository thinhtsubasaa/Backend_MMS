using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class DM_HangMuc : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("DM_TanSuat")]
        public Guid? TanSuat_Id { get; set; }
        public DM_TanSuat DM_TanSuat { get; set; }
        public string NoiDungBaoDuong { get; set; }
        public int DinhMuc { get; set; }
        public string LoaiBaoDuong { get; set; }
        public string GhiChu { get; set; }
        [ForeignKey("DM_Loai")]
        public Guid? LoaiPT_Id { get; set; }
        public DM_Loai DM_Loai { get; set; }
        public int CanhBao_DenHan { get; set; }
        public int CanhBao_GanDenHan { get; set; }

    }
}
