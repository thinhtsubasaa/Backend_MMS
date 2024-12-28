using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class PhuongTien_DonVi : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [ForeignKey("PhuongTien")]
        public Guid? PhuongTien_Id { get; set; }
        public DS_PhuongTien PhuongTien { get; set; }

        [ForeignKey("DonVi")]
        public Guid? DonVi_Id { get; set; }
        public DM_DonVi DonVi { get; set; }

        [ForeignKey("BoPhan")]
        public Guid? BoPhan_Id { get; set; }
        public DM_BoPhan BoPhan { get; set; }
        public DateTime? Date_From { get; set; }
        public DateTime? Date_To { get; set; }
        public string Status { get; set; }
        public string Reason_Note { get; set; }
    }
}
