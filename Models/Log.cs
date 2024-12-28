using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ERP.Data.MyDbContext;

namespace ERP.Models
{
  public class Log
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [StringLength(500)]
    public string Type { get; set; }
    [StringLength(500)]
    public string Url { get; set; }
    public string Data { get; set; }
    [StringLength(150)]
    public string IpAddress { get; set; }
    public DateTime AccessDate { get; set; }
    [ForeignKey("ApplicationUser")]
    public Guid? AccessdBy { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
  }
}