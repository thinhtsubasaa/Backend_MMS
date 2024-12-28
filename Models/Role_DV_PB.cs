using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Role_DV_PB
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ForeignKey("RoleByDonVi")]
        public Guid RoleByDonVi_Id { get; set; }
        public RoleByDonVi RoleByDonVi { get; set; }
        [ForeignKey("DonVi")]
        public Guid? DonVi_Id { get; set; }
        public DonVi DonVi { get; set; }
        [ForeignKey("Phongban")]
        public Guid? Phongban_Id { get; set; }
        public Phongban Phongban { get; set; }
        [ForeignKey("BoPhan")]
        public Guid? BoPhan_Id { get; set; }
        public BoPhan BoPhan { get; set; }
        [ForeignKey("TapDoan")]
        public Guid? TapDoan_Id { get; set; }
        public TapDoan TapDoan { get; set; }
    }
}
