using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ERP.Models;
using ERP.Repositories;
using static ERP.Data.MyDbContext;

namespace ERP.Data
{
  public class MyDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>,
    ApplicationUserRole, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>

  {
    public class ApplicationUser : IdentityUser<Guid>
    {
      public string FullName { get; set; }
      public string MaNhanVien { get; set; }
      [StringLength(500)]
      public string HinhAnhUrl { get; set; }
      public string ChucDanh { get; set; }
      public bool IsActive { get; set; }
      //thêm trường để xác nhận cbnv này được cấp tài khoản
      public bool? AccountLevel { get; set; }
      public bool MustChangePass { get; set; }
      public bool IsDeleted { get; set; }
      public DateTime? CreatedDate { get; set; }
      public DateTime? UpdatedDate { get; set; }
      public DateTime? DeletedDate { get; set; }
      public ICollection<ApplicationUserRole> UserRoles { get; set; }
      public Guid? DonViTraLuong_Id { get; set; }
      public bool NghiViec { get; set; }
      public DateTime? NgayNghiViec { get; set; }
      [StringLength(50)]
      public string GhiChu { get; set; }
      public string LoaiUser { get; set; }
      public string MaPin { get; set; }
      public string TenPhongBan { get; set; }
      public string ChuoiPhongBan { get; set; }
      public string ChucVu { get; set; }
      public string TrangThai { get; set; }

    }
    public class ApplicationRole : IdentityRole<Guid>
    {
      public string Description { get; set; }
      public bool IsDeleted { get; set; }
      public DateTime? CreatedDate { get; set; }
      public DateTime? UpdatedDate { get; set; }
      public DateTime? DeletedDate { get; set; }
      public Guid? PhanMem_Id { get; set; }
      public Guid? TapDoan_Id { get; set; }
      public Guid? DonVi_Id { get; set; }
      public Guid? PhongBan_Id { get; set; }
      public ICollection<ApplicationUserRole> UserRoles { get; set; }
      public ICollection<Menu_Role> Menu_Roles { get; set; }
    }
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
      public virtual ApplicationUser User { get; set; }
      public virtual ApplicationRole Role { get; set; }
      public bool Default { get; set; } = false;
      public bool IsActive_Role { get; set; } = true;
    }
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);
      //Loại bỏ quan hệ vòng
      foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
      {
        relationship.DeleteBehavior = DeleteBehavior.Restrict;
      }
      builder.Entity<ApplicationUserRole>(userRole =>
        {
          userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

          userRole.HasOne(ur => ur.Role)
              .WithMany(r => r.UserRoles)
              .HasForeignKey(ur => ur.RoleId)
              .IsRequired();

          userRole.HasOne(ur => ur.User)
              .WithMany(r => r.UserRoles)
              .HasForeignKey(ur => ur.UserId)
              .IsRequired();
        });
      builder.Entity<Menu_Role>(pq =>
      {
        pq.HasKey(ur => new { ur.Menu_Id, ur.Role_Id });

        pq.HasOne(ur => ur.Menu)
            .WithMany(r => r.Menu_Roles)
            .HasForeignKey(ur => ur.Menu_Id)
            .IsRequired();

        pq.HasOne(ur => ur.Role)
            .WithMany(r => r.Menu_Roles)
            .HasForeignKey(ur => ur.Role_Id)
            .IsRequired();
      });
    }

    public DbSet<Menu> Menus { get; set; }
    public DbSet<ChucVu> ChucVus { get; set; }
    public DbSet<Phongban> phongbans { get; set; }
    public DbSet<DonVi> DonVis { get; set; }
    public DbSet<Menu_Role> Menu_Roles { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<BoPhan> BoPhans { get; set; }
    public DbSet<TapDoan> TapDoans { get; set; }
    public DbSet<DieuChuyenNhanVien> DieuChuyenNhanViens { get; set; }
    public DbSet<Config> Configs { get; set; }
    public DbSet<RoleByDonVi> RoleByDonVis { get; set; }
    public DbSet<Role_DV_PB> Role_DV_PBs { get; set; }
    public DbSet<ChiTiet_DV_PB_BP> ChiTiet_DV_PB_BPs { get; set; }
    public DbSet<NhomDoiTac> NhomDoiTacs { get; set; }

    public DbSet<TaiXe> TaiXes { get; set; }
    public DbSet<Adsun> Adsuns { get; set; }
    public DbSet<DS_PhuongTien> DS_PhuongTiens { get; set; }
    public DbSet<PhuongTien_PhuTrach> PhuongTien_PhuTrachs { get; set; }
    public DbSet<PhuongTien_DonVi> PhuongTien_DonVis { get; set; }
    public DbSet<DS_ThietBi> DS_ThietBis { get; set; }
    public DbSet<DM_DonVi> DM_DonVis { get; set; }
    public DbSet<DM_BoPhan> DM_BoPhans { get; set; }
    public DbSet<DM_Nhom> DM_Nhoms { get; set; }
    public DbSet<DM_Loai> DM_Loais { get; set; }
    public DbSet<DM_Model> DM_Models { get; set; }
    public DbSet<DM_TinhTrang> DM_TinhTrangs { get; set; }
    public DbSet<Translate> Translates { get; set; }
    public DbSet<Status> Statuss { get; set; }

  }
}