using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using static ERP.Data.MyDbContext;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Data
{

  public class MyIdentityDataInitializer
  {
    public static IUnitofWork uow;
    public MyIdentityDataInitializer(IUnitofWork _uow)
    {
      uow = _uow;
    }
    public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
      if (!await roleManager.RoleExistsAsync("Administrator"))
      {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Administrator", Description = "Quản trị", CreatedDate = DateTime.Now });
      }
      if (await userManager.FindByNameAsync("levinhdu") == null)
      {
        ApplicationUser user = new ApplicationUser();
        user.Id = Guid.Parse("c662783d-03c0-4404-9473-1034f1ac1caa");
        user.UserName = "buiquoctinh";
        user.Email = "buiquoctinh@thilogi.com.vn";
        user.FullName = "Bùi Quốc Tịnh";
        user.MaNhanVien = "2302091";
        user.IsActive = true;
        user.CreatedDate = DateTime.Now;
        user.PasswordHash = Commons.HashPassword("Abc@2017");
        IdentityResult result = await userManager.CreateAsync(user);
        if (result.Succeeded)
        {
          userManager.AddToRoleAsync(user, "Administrator").Wait();
        }
      }
    }
  }
}