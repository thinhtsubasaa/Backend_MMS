using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class DM_Loai : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Name_Eng { get; set; }
        public string ThuocNhom { get; set; }
        public string Note { get; set; }
        public string Img { get; set; }

    }
}