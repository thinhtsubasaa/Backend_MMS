using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using static ERP.Controllers.DonViController;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BoPhanController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public BoPhanController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }

        [HttpGet]
        public ActionResult Get(string keyword, int page = 1, Guid? phongbanid = null)
        {
            if (keyword == null) keyword = "";
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();


            var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
            var roles = userManager.GetRolesAsync(exit).Result;
            var isAdmin = roles.Contains("Administrator");
            if (isAdmin)
            {
                string[] include = { "Phongban", "Phongban.DonVi" };
                var data = uow.BoPhans.GetAll(t => !t.IsDeleted
                && (phongbanid == null || t.PhongBan_Id == phongbanid)
                && (t.MaBoPhan.ToLower().Contains(keyword.ToLower()) || t.TenBoPhan.ToLower().Contains(keyword.ToLower())), null, include
                    ).Select(x => new
                    {
                        x.Id,
                        x.MaBoPhan,
                        x.TenBoPhan,
                        x.Phongban.MaPhongBan,
                        x.Phongban.TenPhongBan,
                        x.Phongban.DonVi.TenDonVi,
                        x.BoPhan_Id
                    });
                if (data == null)
                {
                    return NotFound();
                }
                if (page == -1)
                {
                    return Ok(data);
                }
                else
                {
                    int totalRow = data.Count();
                    int pageSize = pageSizeData[0].PageSize;
                    int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);

                    // Kiểm tra và điều chỉnh giá trị của page
                    if (page < 1)
                    {
                        page = 1;
                    }
                    else if (page > totalPage)
                    {
                        page = totalPage;
                    }

                    var datalist = data.Skip((page - 1) * pageSize).Take(pageSize);
                    return Ok(new
                    {
                        totalRow,
                        totalPage,
                        pageSize,
                        datalist
                    });
                }
            }
            else
            {
                var appUser = userManager.FindByIdAsync(User.Identity.Name).Result;
                var roledv = uow.roleByDonVis.GetAll(x => !x.IsDeleted && x.User_Id == appUser.Id).ToList();
                var roleIds = roledv.Select(rv => rv.Id).ToList();
                var role = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => x.BoPhan_Id).Select(g => g.First()).ToList();
                var responseDataList = new List<object>(); // Danh sách dữ liệu từ bảng khác

                foreach (var rol in role)
                {
                    string[] include = { "Phongban", "Phongban.DonVi" };
                    var data = uow.BoPhans.GetAll(t => !t.IsDeleted
                    && (phongbanid == null || t.PhongBan_Id == phongbanid)
                    && (role.Count > 0 && t.Id == rol.BoPhan_Id)
                    && (t.MaBoPhan.ToLower().Contains(keyword.ToLower()) || t.TenBoPhan.ToLower().Contains(keyword.ToLower())), null, include
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaBoPhan,
                            x.TenBoPhan,
                            x.Phongban.MaPhongBan,
                            x.Phongban.TenPhongBan,
                            x.Phongban.DonVi.TenDonVi,
                            x.BoPhan_Id
                        });

                    if (data != null)
                    {
                        responseDataList.AddRange(data.Distinct(new DataEqualityComparer()));

                    }
                }

                if (responseDataList.Count == 0)
                {
                    return Ok(responseDataList);
                }

                if (page == -1)
                {
                    return Ok(responseDataList);
                }
                else
                {
                    int totalRow = responseDataList.Count();
                    int pageSize = pageSizeData[0].PageSize;
                    int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);

                    // Kiểm tra và điều chỉnh giá trị của page
                    if (page < 1)
                    {
                        page = 1;
                    }
                    else if (page > totalPage)
                    {
                        page = totalPage;
                    }

                    var datalist = responseDataList.Skip((page - 1) * pageSize).Take(pageSize);
                    return Ok(new
                    {
                        totalRow,
                        totalPage,
                        pageSize,
                        datalist
                    });
                }
            }
        }

        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            var query = uow.BoPhans.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.MaBoPhan,
                x.TenBoPhan,
                x.BoPhan_Id,
                x.PhongBan_Id
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(ClassBoPhan data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.BoPhans.Exists(x => x.MaBoPhan == data.MaBoPhan && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaBoPhan + " đã tồn tại trong hệ thống");
                else if (uow.BoPhans.Exists(x => x.MaBoPhan == data.MaBoPhan && x.IsDeleted))
                {
                    var d = uow.BoPhans.GetAll(x => x.MaBoPhan == data.MaBoPhan).FirstOrDefault();
                    var max_thutu = uow.BoPhans.GetAll(x => x.Id == data.BoPhan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaBoPhan = data.MaBoPhan;
                    d.TenBoPhan = data.TenBoPhan;
                    d.PhongBan_Id = data.PhongBan_Id;
                    d.BoPhan_Id = data.BoPhan_Id;
                    d.ThuTu = max_thutu + 1;
                    uow.BoPhans.Update(d);
                }
                else
                {
                    var max_thutu = uow.BoPhans.GetAll(x => x.Id == data.BoPhan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    BoPhan bp = new BoPhan();
                    Guid id = Guid.NewGuid();
                    bp.Id = id;
                    bp.MaBoPhan = data.MaBoPhan;
                    bp.TenBoPhan = data.TenBoPhan;
                    bp.PhongBan_Id = data.PhongBan_Id;
                    bp.CreatedDate = DateTime.Now;
                    bp.CreatedBy = Guid.Parse(User.Identity.Name);
                    bp.BoPhan_Id = data.BoPhan_Id;
                    bp.ThuTu = max_thutu + 1;
                    uow.BoPhans.Add(bp);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, ClassBoPhan data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (id != data.Id)
                {
                    return BadRequest();
                }
                if (uow.BoPhans.Exists(x => x.MaBoPhan == data.MaBoPhan && x.Id != data.Id && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaBoPhan + " đã tồn tại trong hệ thống");
                else if (uow.BoPhans.Exists(x => x.MaBoPhan == data.MaBoPhan && x.IsDeleted))
                {
                    var d = uow.BoPhans.GetAll(x => x.MaBoPhan == data.MaBoPhan).FirstOrDefault();
                    var max_thutu = uow.BoPhans.GetAll(x => x.Id == data.BoPhan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaBoPhan = data.MaBoPhan;
                    d.TenBoPhan = data.TenBoPhan;
                    d.PhongBan_Id = data.PhongBan_Id;
                    d.BoPhan_Id = data.BoPhan_Id;
                    d.ThuTu = max_thutu + 1;
                    uow.BoPhans.Update(d);

                }
                else
                {
                    var max_thutu = uow.BoPhans.GetAll(x => x.Id == data.BoPhan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    var d = uow.BoPhans.GetAll(x => x.Id == id).FirstOrDefault();
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaBoPhan = data.MaBoPhan;
                    d.TenBoPhan = data.TenBoPhan;
                    d.PhongBan_Id = data.PhongBan_Id;
                    d.BoPhan_Id = data.BoPhan_Id;
                    d.ThuTu = max_thutu + 1;
                    uow.BoPhans.Update(d);
                }

                uow.Complete();
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                BoPhan duLieu = uow.BoPhans.GetById(id);
                var query = uow.chiTiet_DV_PB_BPs.GetAll(x => x.BoPhan_Id == id);
                if (query.Count() == 0)
                {
                    if (duLieu == null)
                    {
                        return NotFound();
                    }
                    duLieu.DeletedDate = DateTime.Now;
                    duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                    duLieu.IsDeleted = true;
                    uow.BoPhans.Update(duLieu);
                    uow.Complete();
                    return Ok(duLieu);
                }
                return StatusCode(StatusCodes.Status409Conflict, "Bạn chỉ có thể chỉnh sửa thông tin này");
            }

        }
        [HttpDelete("Remove/{id}")]
        public ActionResult Delete_Remove(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                uow.BoPhans.Delete(id);
                uow.Complete();
                return Ok();
            }
        }

        /*        [HttpPost("KiemTraDuLieuImport")]
                public ActionResult KiemTraDuLieuImport(List<ClassBoPhanImport> data)
                {
                    var phongban = uow.phongbans.GetAll(x => !x.IsDeleted);
                    foreach (var item in data)
                    {
                        item.ClassName = "new";
                        var pb = phongban.FirstOrDefault(x => x.MaPhongBan.ToLower() == item.MaPhongBan.ToLower());
                        if (pb == null)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã chưa có trong danh mục ";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                        else item.PhongBan_Id = pb.Id;
                        // Kiểm tra mã bộ phận không vượt quá 50 kí tự
                        if (item.MaBoPhan.Length > 50)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã bộ phận vượt quá giới hạn kí tự cho phép (50 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }

                        // Kiểm tra tên bộ phận không vượt quá 250 kí tự
                        if (item.TenBoPhan.Length > 250)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Tên bộ phận vượt quá giới hạn kí tự cho phép (250 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }

                        if (uow.BoPhans.Exists(x => x.MaBoPhan == item.MaBoPhan))
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã bộ phận " + item.MaBoPhan + "đã tồn tại trong hệ thống";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                    }
                    return Ok(data);
                }*/

        public class ClassBoPhanImport
        {
            public Guid Id { get; set; }
            public string MaBoPhan { get; set; }
            public string TenBoPhan { get; set; }
            public string MaPhongBan { get; set; }
            public string TenPhongBan { get; set; }
            public Guid? PhongBan_Id { get; set; }
            public Guid? BoPhanCha_Id { get; set; }
            public string MaBoPhanCha { get; set; }
            public string ClassName { get; set; }
            public string GhiChuImport { get; set; }
        }
        [HttpPost("ImportExel")]
        public ActionResult ImportExel(List<ClassBoPhanImport> data)
        {
            var phongban = uow.phongbans.GetAll(x => !x.IsDeleted);
            var bophancha = uow.BoPhans.GetAll(x => !x.IsDeleted);
            foreach (var item in data)
            {
                item.ClassName = "new";
                var pb = phongban.FirstOrDefault(x => x.MaPhongBan.ToLower() == item.MaPhongBan.ToLower());
                if (pb == null)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaBoPhan + "có mã phòng ban chưa có trong danh mục ";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                else item.PhongBan_Id = pb.Id;

                if (!string.IsNullOrEmpty(item.MaBoPhanCha))
                {
                    item.MaBoPhanCha = item.MaBoPhanCha.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                    var bpc = bophancha.FirstOrDefault(x => x.MaBoPhan.ToLower() == item.MaBoPhanCha.ToLower());
                    if (bpc == null)
                    {
                        item.ClassName = "error";
                        item.GhiChuImport += item.MaBoPhanCha + " có mã bộ phận cha chưa có trong danh mục ";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else item.BoPhanCha_Id = bpc.Id;
                }

                // Kiểm tra mã bộ phận không vượt quá 50 kí tự
                if (item.MaBoPhan.Length > 50)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaBoPhan + "có mã bộ phận vượt quá giới hạn kí tự cho phép (50 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }

                // Kiểm tra tên bộ phận không vượt quá 250 kí tự
                if (item.TenBoPhan.Length > 250)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaBoPhan + "có tên bộ phận vượt quá giới hạn kí tự cho phép (250 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }

                if (uow.BoPhans.Exists(x => x.MaBoPhan == item.MaBoPhan && !x.IsDeleted))
                {
                    item.ClassName = "error";
                    item.GhiChuImport += "Mã bộ phận " + item.MaBoPhan + "đã tồn tại trong hệ thống";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                item.MaBoPhan = item.MaBoPhan.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                item.TenBoPhan = item.TenBoPhan.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                if (uow.BoPhans.Exists(x => x.MaBoPhan == item.MaBoPhan && x.IsDeleted))
                {
                    item.ClassName = "renew";
                }

                BoPhan dv = new BoPhan();
                dv.PhongBan_Id = item.PhongBan_Id;
                dv.MaBoPhan = item.MaBoPhan;
                dv.TenBoPhan = item.TenBoPhan;
                dv.BoPhan_Id = item.BoPhanCha_Id;
                var id = Guid.NewGuid();
                if (item.ClassName == "new")
                {
                    dv.Id = id;
                    dv.CreatedBy = Guid.Parse(User.Identity.Name);
                    dv.CreatedDate = DateTime.Now;
                    uow.BoPhans.Add(dv);
                }
                else
                {
                    var bp = uow.BoPhans.GetAll(x => x.MaBoPhan == item.MaBoPhan && x.IsDeleted).FirstOrDefault();
                    bp.PhongBan_Id = item.PhongBan_Id;
                    bp.MaBoPhan = item.MaBoPhan;
                    bp.TenBoPhan = item.TenBoPhan;
                    bp.BoPhan_Id = item.BoPhanCha_Id;
                    bp.UpdatedBy = Guid.Parse(User.Identity.Name);
                    bp.UpdatedDate = DateTime.Now;
                    bp.IsDeleted = false;
                    uow.BoPhans.Update(bp);

                }
            }
            uow.Complete();
            return Ok();
        }

        [HttpPost("ExportFileExcel")]
        public ActionResult ExportFileExcel()
        {
            string fullFilePath = Path.Combine(environment.ContentRootPath, "Uploads/Templates/Danhmucbophan.xlsx");
            //data[0].NgayBanGiao = string.Format("dd/MM/yyyy");
            //string[] arrDate = data[0].NgayBanGiao.Split("/");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    // Add a new worksheet if none exists
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Phòng ban"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet = package.Workbook.Worksheets.Add("Phòng ban");
                }
                int stt = 1;
                int sublistIndex = 0;
                int indexrow = 4;
                string[] include = { "DonVi" };
                var donvi = uow.phongbans.GetAll(x => !x.IsDeleted, null, include).ToList();
                foreach (var item in donvi)
                {
                    worksheet.InsertRow(indexrow, 1, indexrow);
                    worksheet.Row(indexrow).Height = 20;

                    worksheet.Cells["A" + indexrow].Value = stt;
                    worksheet.Cells["B" + indexrow].Value = item.MaPhongBan;
                    worksheet.Cells["C" + indexrow].Value = item.TenPhongBan;
                    worksheet.Cells["D" + indexrow].Value = item.DonVi.TenDonVi;
                    worksheet.Cells["E" + indexrow].Value = GetTenPhongBan(item?.PhongBan_Id);
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet.Cells[indexrow, col];
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 4)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet.Cells["A" + indexrow];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt++;
                    indexrow++;
                    sublistIndex++;

                }
                ExcelWorksheet worksheet3 = package.Workbook.Worksheets["Danh mục bộ phận"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet3 == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet3 = package.Workbook.Worksheets.Add("Danh mục bộ phận");
                }
                int stt3 = 1;
                int indexrow3 = 4;
                string[] include3 = { "Phongban" };
                var bophan = uow.BoPhans.GetAll(x => !x.IsDeleted, null, include3).ToList();
                foreach (var item in bophan)
                {
                    worksheet3.InsertRow(indexrow3, 1, indexrow3);
                    worksheet3.Row(indexrow3).Height = 20;
                    worksheet3.Cells["A" + indexrow3].Value = stt3;
                    worksheet3.Cells["B" + indexrow3].Value = item.MaBoPhan;
                    worksheet3.Cells["C" + indexrow3].Value = item.TenBoPhan;
                    worksheet3.Cells["D" + indexrow3].Value = item.Phongban.TenPhongBan;
                    worksheet3.Cells["E" + indexrow3].Value = GetMaBoPhan(item?.BoPhan_Id);
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet3.Cells[indexrow3, col];
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        cell.Style.WrapText = true;
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 4)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA = worksheet3.Cells["A" + indexrow3];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt3++;
                    indexrow3++;
                }
                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }

        public class DropdownTreeNode_BoPhan
        {
            public Guid Id { get; set; }
            public string NameId { get; set; }
            public string MaBoPhan { get; set; }
            public string TenBoPhan { get; set; }
            public int ThuTu { get; set; }
            public string STT { get; set; }
            public Guid? BoPhan_Id { get; set; }
            public Guid? PhongBan_Id { get; set; }
            public string TenPhongBan { get; set; }
            public string TenDonVi { get; set; }
            public List<DropdownTreeNode_BoPhan> Children { get; set; }
            public bool? Disable { get; set; }
        }
        [HttpGet("bo-phan-tree")]
        public ActionResult<List<DropdownTreeNode_BoPhan>> GetBoPhanTree(string keyword, Guid? donviid,Guid? phongBan_Id)
        {
            var rootBoPhanList = uow.BoPhans.GetAll(x => !x.IsDeleted && x.BoPhan_Id == null).ToList();
            var result = new List<DropdownTreeNode_BoPhan>();
            var topLevelOrder = 1;
            foreach (var rootBoPhan in rootBoPhanList)
            {
                var rootNode = CreateTreeNode(rootBoPhan, topLevelOrder.ToString(), rootBoPhan.ThuTu,keyword, donviid,phongBan_Id);
                topLevelOrder++;
                if ((string.IsNullOrEmpty(keyword) || rootNode.MaBoPhan.ToLower().Contains(keyword.ToLower()) || rootNode.TenBoPhan.ToLower().Contains(keyword.ToLower())) && (donviid == null || GetDonViIdByPhongBanId(rootNode.PhongBan_Id) == donviid) && (phongBan_Id == null || rootNode.PhongBan_Id == phongBan_Id))
                {
                    result.Add(rootNode);
                }
            }
            return Ok(result);
        }

        private DropdownTreeNode_BoPhan CreateTreeNode(BoPhan parentBoPhan, string parentOrder, int ThuTu, string keyword, Guid? donviid, Guid? phongBan_Id)
        {
            var treeNode = new DropdownTreeNode_BoPhan
            {
                Id = parentBoPhan.Id,
                MaBoPhan = parentBoPhan.MaBoPhan,
                TenBoPhan = parentBoPhan.TenBoPhan,
                BoPhan_Id = parentBoPhan.BoPhan_Id,
                PhongBan_Id = parentBoPhan?.PhongBan_Id,
                TenPhongBan = GetTenPhongBan(parentBoPhan?.PhongBan_Id),
                TenDonVi = GetTenDonViByPhongBanId(parentBoPhan?.PhongBan_Id),
                ThuTu = parentBoPhan.ThuTu,
                STT = $"{parentOrder}",
                Children = new List<DropdownTreeNode_BoPhan>()
            };
            var childOrder = 1;
            var childBoPhanList = uow.BoPhans.GetAll(x => !x.IsDeleted && x.BoPhan_Id == parentBoPhan.Id).OrderBy(x => x.BoPhan_Id).ThenBy(x => x.ThuTu).ToList();
            foreach (var childBoPhan in childBoPhanList)
            {
                var childNode = CreateTreeNode(childBoPhan, $"{parentOrder}.{childOrder}", childBoPhan.ThuTu, keyword,donviid,phongBan_Id);
                if ((phongBan_Id == null || childNode.PhongBan_Id==phongBan_Id) && (donviid == null || GetDonViIdByPhongBanId(childNode.PhongBan_Id) == donviid) && (string.IsNullOrEmpty(keyword) || childNode.MaBoPhan.ToLower().Contains(keyword.ToLower()) || childNode.TenBoPhan.ToLower().Contains(keyword.ToLower())))
                {
                    treeNode.Children.Add(childNode);
                }
                childOrder++;
            }

            return treeNode;
        }

        private Guid? GetDonViIdByPhongBanId(Guid? phongban_Id)
        {
            if (phongban_Id.HasValue)
            {
                var phongban = uow.phongbans.GetById(phongban_Id.Value);
                if (phongban != null)
                {
                    return phongban.DonVi_Id;
                }
            }
            return null;
        }        
        private string GetTenDonViByPhongBanId(Guid? phongban_Id)
        {
            if (phongban_Id.HasValue)
            {
                var phongban = uow.phongbans.GetById(phongban_Id.Value);
                var donvi = uow.DonVis.FirstOrDefault(x => x.Id == phongban.DonVi_Id);
                if (donvi != null)
                {
                    return donvi.TenDonVi;
                }
            }
            return null;
        }

        private string GetTenPhongBan(Guid? phongban_Id)
        {
            if (phongban_Id.HasValue)
            {
                var phongban = uow.phongbans.GetById(phongban_Id.Value);
                if (phongban != null)
                {
                    return phongban.TenPhongBan;
                }
            }
            return null;
        }
        private string GetMaBoPhan(Guid? bophan_Id)
        {
            if (bophan_Id.HasValue)
            {
                var bp = uow.BoPhans.GetById(bophan_Id.Value);
                if (bp != null)
                {
                    return bp.MaBoPhan;
                }
            }
            return null;
        }
    }
}