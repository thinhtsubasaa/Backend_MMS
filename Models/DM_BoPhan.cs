using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class DM_BoPhan : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ForeignKey("DonVi")]
        public Guid? DonVi_Id { get; set; }
        public DM_DonVi DonVi { get; set; }
        public string Name { get; set; }
        public string Name_Eng { get; set; }
        public string Note { get; set; }
        public string MaBP { get; set; }

    }
}
