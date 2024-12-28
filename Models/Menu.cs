using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class Menu:Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [StringLength(250)]
        [Required(ErrorMessage = "Tên bắt buộc")]
        public string TenMenu { get; set; }
        [StringLength(250)]
        [Required(ErrorMessage = "Url bắt buộc")]
        public string Url { get; set; }
        public Guid? Parent_Id { get; set; }
        public bool isMoBi { get; set; }
        public int ThuTu { get; set; }
        [StringLength(50)]
        [Required(ErrorMessage = "Icon bắt buộc")]
        public string Icon { get; set; }
        [JsonIgnore]
        public virtual ICollection<Menu_Role> Menu_Roles { get; set; }
        public Guid? PhanMem_Id { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public Guid? DonVi_id { get; set; }
        public Guid? PhongBan_Id { get; set; }
    }
}