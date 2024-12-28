using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class TaiXe : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string MaTaiXe { get; set; }
        public string TenTaiXe { get; set; }
        public string SoDienThoai { get; set; }
        public string HangBang { get; set; }
        public bool isVaoCong { get; set; }




    }
}