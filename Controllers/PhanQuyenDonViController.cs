using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhanQuyenDonViController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public PhanQuyenDonViController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }
        [HttpPost]
        public IActionResult PhanQuyen(PhanQuyenDonVi viewModel)
        {
            if (ModelState.IsValid)
            {
                // Lấy danh sách đơn vị từ database
                List<DonViViewModel> donViList = new List<DonViViewModel>();

                // Lấy danh sách người dùng từ database
                List<ApplicationUser> userList = new List<ApplicationUser>();

                // Lấy người dùng cần phân quyền
                ApplicationUser user = userList.FirstOrDefault(u => u.Id == viewModel.User_Id);

                if (user != null)
                {
                    // Xóa tất cả các phân quyền cũ của người dùng
                    // (ở đây giả sử phân quyền lưu trong bảng UserRoles của Identity)
                    var userRoles = userManager.GetRolesAsync(user).Result;
                    userManager.RemoveFromRolesAsync(user, userRoles).Wait();

                    // Lặp qua danh sách đơn vị và phân quyền người dùng cho từng đơn vị
                    foreach (var donVi in viewModel.lst_DonVis)
                    {
                        if (donVi.Checked)
                        {
                            // Phân quyền người dùng cho đơn vị
                            // (ở đây giả sử phân quyền lưu trong bảng UserRoles của Identity)
                            //var role = GetRoleForDonVi(donVi.); // Hàm này trả về vai trò tương ứng với đơn vị
                            //userManager.AddToRoleAsync(user, role.Name).Wait();
                        }
                    }

                    // TODO: Lưu các thay đổi vào database (nếu cần)

                    return Ok(); // Trả về kết quả thành công
                }
            }

            return BadRequest(); // Trả về kết quả lỗi nếu dữ liệu không hợp lệ
        }


        private ApplicationRole GetRoleForDonVi(Guid donViId)
        {
            // Code để lấy vai trò tương ứng với đơn vị từ database ở đây
            // Sử dụng Entity Framework, ADO.NET, hoặc các công nghệ tương tự để truy vấn và lấy dữ liệu từ bảng vai trò
            // Rồi trả về vai trò tương ứng với đơn vị
            var donvi = uow.DonVis.GetAll(x => !x.IsDeleted).ToArray();
            ApplicationRole role = new ApplicationRole
            {
                // Ví dụ tạo một vai trò với tên là "RoleDV001" tương ứng với đơn vị có Id là donViId
                Id = donvi[0].Id,
                Description = donvi[0].MaDonVi,
                IsDeleted = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            return role;
        }

    }
}
