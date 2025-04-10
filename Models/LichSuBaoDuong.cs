using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class LichSuBaoDuong : Auditable
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
        public string NoiDung { get; set; }
        public string KetQua { get; set; }
        public DateTime Ngay { get; set; }
        public Guid? DiaDiem_Id { get; set; }
        public int ChiPhi { get; set; }
        public string TrangThai { get; set; }
        public string NguoiYeuCau { get; set; }
        public string NguoiXacNhan { get; set; }
        public string LyDo { get; set; }
        public Guid? NguoiYeuCau_Id { get; set; }
        public Guid? NguoiXacNhan_Id { get; set; }
        public DateTime? NgayXacNhan { get; set; }
        public int SoKM { get; set; }
        public bool IsYeuCau { get; set; }
        public bool IsDuyet { get; set; }
        public bool IsBaoDuong { get; set; }
        public bool IsLenhHoanThanh { get; set; }
        public bool IsHoanThanh { get; set; }
        public DateTime? NgayDiBaoDuong { get; set; }
        public DateTime? NgayHoanThanh { get; set; }
        public Guid? NguoiDiBaoDuong_Id { get; set; }
        public string NguoiDiBaoDuong { get; set; }
        public Guid? NguoiXacNhanHoanThanh_Id { get; set; }
        public string NguoiXacNhanHoanThanh { get; set; }
        public string HinhAnh { get; set; }
        public string NguoiHuyDuyet { get; set; }
        public Guid? NguoiHuyDuyet_Id { get; set; }
        public string GhiChu { get; set; }
        public int ThoiGianSuDung { get; set; }
        public DateTime? NgayDeXuatHoanThanh { get; set; }
        public Guid? NguoiDeXuatHoanThanh_Id { get; set; }
        public string NguoiDeXuatHoanThanh { get; set; }
        public int ChiPhi_TD { get; set; }

        [JsonIgnore]
        public virtual List<LichSuBaoDuong_ChiTiet> LichSuBaoDuong_ChiTiets { get; set; }


    }
}
