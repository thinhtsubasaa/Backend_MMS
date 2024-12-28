using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.ComponentModel.DataAnnotations;
using static ERP.Data.MyDbContext;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ERP.Models
{
    public class RoleByDonVi : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [ForeignKey("User")]
        public Guid User_Id { get; set; }
        public ApplicationUser User { get; set; }
        [JsonIgnore]
        public virtual ICollection<Role_DV_PB> Role_DV_PBs { get; set; }
        [NotMapped]
        public List<Role_DV_PB> Lstrole { get; set; }
    }
}
