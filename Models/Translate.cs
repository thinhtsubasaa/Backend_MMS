using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
    public class Translate : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public string Text { get; set; }
        public string VI { get; set; }
        public string EN { get; set; }
        public string ZH_TW { get; set; }
        public string JA { get; set; }
        public string Group { get; set; }

    }
}
