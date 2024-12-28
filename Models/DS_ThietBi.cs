using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public string NgayBatDau { get; set; }
        public string Note { get; set; }
    }
}
