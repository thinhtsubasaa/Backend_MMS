using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class DS_PhuongTien : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string BienSo1 { get; set; }
        public string BienSo2 { get; set; }
        public string DonViSuDung { get; set; }
        public string HinhAnh { get; set; }
        public string TinhTrang { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public string Note { get; set; }
        public string LoaiPT { get; set; }
        public string Model { get; set; }
        public string Model_Option { get; set; }
        public string KLBT { get; set; }
        public string KLHH { get; set; }
        public string KLTB { get; set; }
        public string KLKT { get; set; }
        public string Address_nearest { get; set; }
        public string ViTri { get; set; }
        public string ViTri_Lat { get; set; }
        public string ViTri_Long { get; set; }
        public string MaPhuongTien { get; set; }
        [ForeignKey("DM_Model")]
        public Guid? Model_Id { get; set; }
        public DM_Model DM_Model { get; set; }

        public Guid? DonVi_Id { get; set; }

        [ForeignKey("DM_Loai")]
        public Guid? LoaiPT_Id { get; set; }
        public DM_Loai DM_Loai { get; set; }
        [ForeignKey("DM_TinhTrang")]
        public Guid? TinhTrang_Id { get; set; }
        public DM_TinhTrang DM_TinhTrang { get; set; }
        public int SoKM { get; set; }
        public int SoChuyenXe { get; set; }
        public string SoKhung { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public int SoKM_Adsun { get; set; }
        public int TongSoKM_Adsun { get; set; }
        [ForeignKey("LichSuBaoDuong")]
        public Guid? LichSuBaoDuong_Id { get; set; }
        public LichSuBaoDuong LichSuBaoDuong { get; set; }
        [ForeignKey("MMS_PhuTrachBoPhan")]
        public Guid? PhuTrachBoPhan_Id { get; set; }
        public MMS_PhuTrachBoPhan PhuTrachBoPhan { get; set; }
        public int SoKM_NgayBaoDuong { get; set; }


    }

}
