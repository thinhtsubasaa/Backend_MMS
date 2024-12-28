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
    }
}
