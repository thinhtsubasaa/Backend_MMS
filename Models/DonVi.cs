using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class DonVi : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [StringLength(50)]
        [Required(ErrorMessage = "Mã bắt buộc")]
        public string MaDonVi { get; set; }
        [StringLength(250)]
        [Required(ErrorMessage = "Tên bắt buộc")]
        public string TenDonVi { get; set; }
        [ForeignKey("TapDoan")]
        public Guid? TapDoan_Id { get; set; }
        public TapDoan TapDoan { get; set; }
        public int ThuTu { get; set; }
        public Guid? DonVi_Id { get; set; }

    }
}