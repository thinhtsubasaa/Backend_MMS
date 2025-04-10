using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERP.Controllers;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class DS_ThietBi : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string MaThietBi { get; set; }
        public string MaCode_BienSo1 { get; set; }
        public string MaCode_BienSo2 { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string LoaiTB { get; set; }
        public string PhanBo { get; set; }
        public string ViTri { get; set; }
        public string ViTri_Lat { get; set; }
        public string ViTri_Long { get; set; }
        public string TinhTrang { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public string Note { get; set; }
        [ForeignKey("DM_Loai")]
        public Guid? LoaiTB_Id { get; set; }
        public DM_Loai DM_Loai { get; set; }

        [ForeignKey("DM_Model")]
        public Guid? Model_Id { get; set; }
        public DM_Model DM_Model { get; set; }

        public int ThoiGianSuDung { get; set; }
        [ForeignKey("DM_TinhTrang")]
        public Guid? TinhTrang_Id { get; set; }
        public DM_TinhTrang DM_TinhTrang { get; set; }
        [ForeignKey("LichSuBaoDuong")]
        public Guid? LichSuBaoDuong_Id { get; set; }
        public LichSuBaoDuong LichSuBaoDuong { get; set; }
        public int ThoiGian_NgayBaoDuong { get; set; }


    }
}
