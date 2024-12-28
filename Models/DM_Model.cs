using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class DM_Model : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Option { get; set; }
        public string Code { get; set; }
        public string Name_Eng { get; set; }
        public string Type { get; set; }
        public string PairingAbility { get; set; }
        public string KLBT { get; set; }
        public string TTMK_KLHH { get; set; }
        public string KLTB { get; set; }
        public string KLKT { get; set; }
        public string Note { get; set; }
        public string HinhAnh { get; set; }

    }
}
