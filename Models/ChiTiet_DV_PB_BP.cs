using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class ChiTiet_DV_PB_BP : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid User_Id { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public Guid? ChucVu_Id { get; set; }
    }
}
