using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class LichSuPhanCongPhuTrach : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string LoaiBaoDuong { get; set; }
        public string TanSuat { get; set; }
        public string GiaTri { get; set; }
        public string Note { get; set; }


    }
}
