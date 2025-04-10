using ERP.Models;
using Microsoft.EntityFrameworkCore;

public class SecondDbContext : DbContext
{
    public SecondDbContext(DbContextOptions<SecondDbContext> options) : base(options) { }

    public DbSet<DonVi_Master> DonVis { get; set; }
    public DbSet<DiaLy_DiaDiem> DiaLy_DiaDiems { get; set; }
    public DbSet<ThongBao_Master> Notifications { get; set; }
    public DbSet<BoPhan_Master> phongbans { get; set; }
    public DbSet<User_Master> ChiTiet_DV_PB_BPs { get; set; }
    public DbSet<UserNet_Master> AspNetUsers { get; set; }
}

