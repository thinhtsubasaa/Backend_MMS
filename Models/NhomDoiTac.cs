using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ERP.Models
{
    public class NhomDoiTac: Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [StringLength(50)]
        [Required(ErrorMessage = "Mã bắt buộc")]
        public string MaNhomDoiTac { get; set; }
        [StringLength(250)]
        [Required(ErrorMessage = "Mã loại bắt buộc")]
        public string TenNhomDoiTac { get; set; }
        public string TenNhomDoiTac_EN { get; set; }
    }
}
