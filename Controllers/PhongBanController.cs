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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using static ERP.Controllers.DonViController;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhongBanController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public PhongBanController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }

        [HttpGet]
        public ActionResult Get(string keyword, int page = 1, Guid? donviid = null)
        {
            if (keyword == null) keyword = "";
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();


            var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
            var roles = userManager.GetRolesAsync(exit).Result;
            var isAdmin = roles.Contains("Administrator");
            if (isAdmin)
            {
                string[] include = { "DonVi" };
                var data = uow.phongbans.GetAll(t => !t.IsDeleted
                && (donviid == null || t.DonVi_Id == donviid)
                && (t.MaPhongBan.ToLower().Contains(keyword.ToLower()) || t.TenPhongBan.ToLower().Contains(keyword.ToLower())), null, include
                    ).Select(x => new
                    {
                        x.Id,
                        x.MaPhongBan,
                        x.TenPhongBan,
                        x.DonVi.MaDonVi,
                        x.DonVi.TenDonVi,
                        x.PhongBan_Id
                    });
                if (data == null)
                {
                    return Ok(data);
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

                    string[] include = { "DonVi" };
                    var data = uow.phongbans.GetAll(t => !t.IsDeleted
                    && (donviid == null || t.DonVi_Id == donviid)
                    && (t.MaPhongBan.ToLower().Contains(keyword.ToLower()) || t.TenPhongBan.ToLower().Contains(keyword.ToLower())), null, include
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaPhongBan,
                            x.TenPhongBan,
                            x.DonVi.MaDonVi,
                            x.DonVi.TenDonVi,
                            x.PhongBan_Id
                        });
                if (data == null)
                {
                    return Ok(data);
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
        }

        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            var query = uow.phongbans.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.MaPhongBan,
                x.TenPhongBan,
                x.PhongBan_Id,
                x.DonVi_Id
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(ClassPhongBan data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.phongbans.Exists(x => x.MaPhongBan == data.MaPhongBan && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaPhongBan + " đã tồn tại trong hệ thống");
                else if (uow.phongbans.Exists(x => x.MaPhongBan == data.MaPhongBan && x.IsDeleted))
                {

                    var d = uow.phongbans.GetAll(x => x.MaPhongBan == data.MaPhongBan).FirstOrDefault();
                    var max_thutu = uow.phongbans.GetAll(x => x.Id == data.PhongBan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaPhongBan = data.MaPhongBan;
                    d.TenPhongBan = data.TenPhongBan;
                    d.PhongBan_Id = data.PhongBan_Id;
                    d.ThuTu = max_thutu + 1;
                    uow.phongbans.Update(d);


                }
                else
                {
                    var max_thutu = uow.phongbans.GetAll(x => x.Id == data.PhongBan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    Phongban pb = new Phongban();
                    Guid id = Guid.NewGuid();
                    pb.Id = id;
                    pb.MaPhongBan = data.MaPhongBan;
                    pb.TenPhongBan = data.TenPhongBan;
                    pb.DonVi_Id = data.DonVi_Id;
                    pb.CreatedDate = DateTime.Now;
                    pb.CreatedBy = Guid.Parse(User.Identity.Name);
                    pb.PhongBan_Id = data.PhongBan_Id;
                    pb.ThuTu = max_thutu + 1;
                    uow.phongbans.Add(pb);
                }
                uow.Complete();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, ClassPhongBan data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // if (id != data.Id)
                // {
                //     return BadRequest();
                // }
                // if (uow.phongbans.Exists(x => x.MaPhongBan == data.MaPhongBan && x.Id != data.Id && !x.IsDeleted))
                //     return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaPhongBan + " đã tồn tại trong hệ thống");
                // else 
                if (uow.phongbans.Exists(x => x.MaPhongBan == data.MaPhongBan && x.IsDeleted))
                {

                    var d = uow.phongbans.GetAll(x => x.MaPhongBan == data.MaPhongBan).FirstOrDefault();
                    var max_thutu = uow.phongbans.GetAll(x => x.Id == data.PhongBan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaPhongBan = data.MaPhongBan;
                    d.TenPhongBan = data.TenPhongBan;
                    d.DonVi_Id = data.DonVi_Id;
                    d.PhongBan_Id = data.PhongBan_Id;
                    d.ThuTu = max_thutu + 1;
                    uow.phongbans.Update(d);

                }
                else
                {
                    var d = uow.phongbans.GetAll(x => x.Id == id).FirstOrDefault();
                    var max_thutu = uow.phongbans.GetAll(x => x.Id == data.PhongBan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaPhongBan = data.MaPhongBan;
                    d.TenPhongBan = data.TenPhongBan;
                    d.DonVi_Id = data?.DonVi_Id;
                    d.PhongBan_Id = data.PhongBan_Id;
                    d.ThuTu = max_thutu + 1;
                    uow.phongbans.Update(d);
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
                Phongban duLieu = uow.phongbans.GetById(id);
                var query = uow.chiTiet_DV_PB_BPs.GetAll(x => x.PhongBan_Id == id);
                if (query.Count() == 0 && !uow.BoPhans.Exists(x => x.PhongBan_Id == id && !x.IsDeleted))
                {
                    if (duLieu == null)
                    {
                        return NotFound();
                    }
                    duLieu.DeletedDate = DateTime.Now;
                    duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                    duLieu.IsDeleted = true;
                    uow.phongbans.Update(duLieu);
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
                uow.phongbans.Delete(id);
                uow.Complete();
                return Ok();
            }
        }

        /*        [HttpPost("KiemTraDuLieuImport")]
                public ActionResult KiemTraDuLieuImport(List<ClassPhongBanImport> data)
                {
                    var donvi = uow.DonVis.GetAll(x => !x.IsDeleted);
                    foreach (var item in data)
                    {
                        item.ClassName = "new";
                        var dv = donvi.FirstOrDefault(x => x.MaDonVi.ToLower() == item.MaDonVi.ToLower());
                        if (dv == null)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã đơn vị không tồn tại";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                        else item.DonVi_Id = dv.Id;
                        // Kiểm tra mã phòng ban không vượt quá 50 kí tự
                        if (item.MaPhongBan.Length > 50)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã phòng ban vượt quá giới hạn kí tự cho phép (50 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }

                        // Kiểm tra tên phòng ban không vượt quá 250 kí tự
                        if (item.TenPhongBan.Length > 250)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Tên phòng ban vượt quá giới hạn kí tự cho phép (250 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                        if (uow.phongbans.Exists(x => x.MaPhongBan == item.MaPhongBan))
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã Phòng Ban " + item.MaPhongBan + "đã tồn tại trong hệ thống";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }

                    }
                    return Ok(data);
                }*/

        public class ClassPhongBanImport
        {
            public Guid Id { get; set; }
            public string MaDonVi { get; set; }
            public string TenDonVi { get; set; }
            public string MaPhongBan { get; set; }
            public string TenPhongBan { get; set; }
            public Guid? DonVi_Id { get; set; }
            public Guid? PhongBanCha_Id { get; set; }
            public string MaPhongBanCha { get; set; }
            public string ClassName { get; set; }
            public string GhiChuImport { get; set; }
        }
        [HttpPost("ImportExel")]
        public ActionResult ImportExel(List<ClassPhongBanImport> data)
        {
            var donvi = uow.DonVis.GetAll(x => !x.IsDeleted);
            var phongbancha = uow.phongbans.GetAll(x => !x.IsDeleted);
            foreach (var item in data)
            {
                item.ClassName = "new";
                var dv = donvi.FirstOrDefault(x => x.MaDonVi.ToLower() == item.MaDonVi.ToLower());
                if (dv == null)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaPhongBan + " có mã đơn vị không tồn tại";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                else item.DonVi_Id = dv.Id;

                if (!string.IsNullOrEmpty(item.MaPhongBanCha))
                {
                    item.MaPhongBanCha = item.MaPhongBanCha.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                    var pbc = phongbancha.FirstOrDefault(x => x.MaPhongBan.ToLower() == item.MaPhongBanCha.ToLower());
                    if (pbc == null)
                    {
                        item.ClassName = "error";
                        item.GhiChuImport += item.MaPhongBanCha + " có mã phòng ban cha chưa có trong danh mục ";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else item.PhongBanCha_Id = pbc.Id;
                }

                // Kiểm tra mã phòng ban không vượt quá 50 kí tự
                if (item.MaPhongBan.Length > 50)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaPhongBan + " có mã phòng ban vượt quá giới hạn kí tự cho phép (50 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }

                // Kiểm tra tên phòng ban không vượt quá 250 kí tự
                if (item.TenPhongBan.Length > 250)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaPhongBan + " có tên phòng ban vượt quá giới hạn kí tự cho phép (250 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                if (uow.phongbans.Exists(x => x.MaPhongBan == item.MaPhongBan && !x.IsDeleted))
                {
                    item.ClassName = "error";
                    item.GhiChuImport += "Mã Phòng Ban " + item.MaPhongBan + " đã tồn tại trong hệ thống";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                item.MaPhongBan = item.MaPhongBan.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                item.TenPhongBan = item.TenPhongBan.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                if (uow.phongbans.Exists(x => x.MaPhongBan == item.MaPhongBan && x.IsDeleted))
                {
                    item.ClassName = "renew";
                }

                Phongban dvs = new Phongban();
                dvs.DonVi_Id = item.DonVi_Id;
                dvs.MaPhongBan = item.MaPhongBan;
                dvs.TenPhongBan = item.TenPhongBan;
                dvs.PhongBan_Id = item?.PhongBanCha_Id;
                var id = Guid.NewGuid();
                if (item.ClassName == "new")
                {
                    dvs.Id = id;
                    dvs.CreatedBy = Guid.Parse(User.Identity.Name);
                    dvs.CreatedDate = DateTime.Now;
                    uow.phongbans.Add(dvs);
                }
                else
                {
                    var pb = uow.phongbans.GetAll(x => x.MaPhongBan == item.MaPhongBan && x.IsDeleted).FirstOrDefault();
                    pb.DonVi_Id = item.DonVi_Id;
                    pb.MaPhongBan = item.MaPhongBan;
                    pb.TenPhongBan = item.TenPhongBan;
                    pb.PhongBan_Id = item?.PhongBanCha_Id;
                    pb.UpdatedBy = Guid.Parse(User.Identity.Name);
                    pb.UpdatedDate = DateTime.Now;
                    pb.IsDeleted = false;
                    uow.phongbans.Update(pb);
                }
            }
            uow.Complete();
            return Ok();
        }

        [HttpPost("ExportFileExcel")]
        public ActionResult ExportFileExcel()
        {
            string fullFilePath = Path.Combine(environment.ContentRootPath, "Uploads/Templates/Danhmucphongban.xlsx");
            //data[0].NgayBanGiao = string.Format("dd/MM/yyyy");
            //string[] arrDate = data[0].NgayBanGiao.Split("/");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    // Add a new worksheet if none exists
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Đơn vị"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet = package.Workbook.Worksheets.Add("Đơn vị");
                }
                int stt = 1;
                int sublistIndex = 0;
                int indexrow = 4;
                string[] include = { "TapDoan" };
                var donvi = uow.DonVis.GetAll(x => !x.IsDeleted, null, include).ToList();
                foreach (var item in donvi)
                {
                    worksheet.InsertRow(indexrow, 1, indexrow);
                    worksheet.Row(indexrow).Height = 20;

                    worksheet.Cells["A" + indexrow].Value = stt;
                    worksheet.Cells["B" + indexrow].Value = item.MaDonVi;
                    worksheet.Cells["C" + indexrow].Value = item.TenDonVi;
                    worksheet.Cells["D" + indexrow].Value = item.TapDoan.TenTapDoan;
                    worksheet.Cells["E" + indexrow].Value = GetMaDonVi(item?.DonVi_Id);
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
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets["Phòng ban"];
                if (worksheet2 == null)
                {
                    worksheet2 = package.Workbook.Worksheets.Add("Phòng ban");
                }
                int stt2 = 1;
                int indexrow2 = 4;
                string[] include2 = { "DonVi" };
                var phongban = uow.phongbans.GetAll(x => !x.IsDeleted, null, include2).ToList();
                foreach (var item in phongban)
                {
                    worksheet2.InsertRow(indexrow2, 1, indexrow2);
                    worksheet2.Row(indexrow2).Height = 20;

                    worksheet2.Cells["A" + indexrow2].Value = stt2;
                    worksheet2.Cells["B" + indexrow2].Value = item.MaPhongBan;
                    worksheet2.Cells["C" + indexrow2].Value = item.TenPhongBan;
                    worksheet2.Cells["D" + indexrow2].Value = item.DonVi.TenDonVi;
                    worksheet2.Cells["E" + indexrow2].Value = GetMaPhongBan(item?.PhongBan_Id);
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet2.Cells[indexrow2, col];
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

                    var cellA = worksheet2.Cells["A" + indexrow2];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt2++;
                    indexrow2++;
                }
                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }

        public class DropdownTreeNode_PhongBan
        {
            public Guid Id { get; set; }
            public string NameId { get; set; }
            public string MaPhongBan { get; set; }
            public string TenPhongBan { get; set; }
            public int ThuTu { get; set; }
            public string STT { get; set; }
            public Guid? PhongBan_Id { get; set; }
            public Guid? DonVi_Id { get; set; }
            public string TenDonVi { get; set; }
            public List<DropdownTreeNode_PhongBan> Children { get; set; }
            public bool? Disable { get; set; }
        }
        [HttpGet("phong-ban-tree")]
        public ActionResult<List<DropdownTreeNode_PhongBan>> GetPhongBanTree(string keyword, Guid? donviid)
        {
            var rootPhongBanList = uow.phongbans.GetAll(x => !x.IsDeleted && x.PhongBan_Id == null).ToList();
            var result = new List<DropdownTreeNode_PhongBan>();
            var topLevelOrder = 1;
            foreach (var rootPhongBan in rootPhongBanList)
            {
                var rootNode = CreateTreeNode(rootPhongBan, topLevelOrder.ToString(), rootPhongBan.ThuTu,keyword,donviid);
                topLevelOrder++;
                if ((string.IsNullOrEmpty(keyword) || rootNode.MaPhongBan.ToLower().Contains(keyword.ToLower()) || rootNode.TenDonVi.ToLower().Contains(keyword.ToLower())) && (donviid == null || rootNode.DonVi_Id == donviid))
                {
                    result.Add(rootNode);
                }
            }
            return Ok(result);
        }

        private DropdownTreeNode_PhongBan CreateTreeNode(Phongban parentPhongBan, string parentOrder, int ThuTu, string keyword, Guid? donviid)
        {
            var treeNode = new DropdownTreeNode_PhongBan
            {
                Id = parentPhongBan.Id,
                MaPhongBan = parentPhongBan.MaPhongBan,
                TenPhongBan = parentPhongBan.TenPhongBan,
                PhongBan_Id = parentPhongBan.PhongBan_Id,
                DonVi_Id = parentPhongBan?.DonVi_Id,
                TenDonVi = GetTenDonVi(parentPhongBan?.DonVi_Id),
                ThuTu = parentPhongBan.ThuTu,
                STT = $"{parentOrder}",
                Children = new List<DropdownTreeNode_PhongBan>()
            };
            var childOrder = 1;
            var childPhongBanList = uow.phongbans.GetAll(x => !x.IsDeleted && x.PhongBan_Id == parentPhongBan.Id).OrderBy(x => x.PhongBan_Id).ThenBy(x => x.ThuTu).ToList();
            foreach (var childPhongBan in childPhongBanList)
            {
                var childNode = CreateTreeNode(childPhongBan, $"{parentOrder}.{childOrder}", childPhongBan.ThuTu, keyword,donviid);
                if ((string.IsNullOrEmpty(keyword) || childNode.MaPhongBan.ToLower().Contains(keyword.ToLower()) || childNode.TenDonVi.ToLower().Contains(keyword.ToLower())) && (donviid == null || childNode.DonVi_Id == donviid))
                {
                    treeNode.Children.Add(childNode);
                }
                childOrder++;
            }

            return treeNode;
        }
        private string GetTenDonVi(Guid? donvi_Id)
        {
            if (donvi_Id.HasValue)
            {
                var donvi = uow.DonVis.GetById(donvi_Id.Value);
                if (donvi != null)
                {
                    return donvi.TenDonVi;
                }
            }
            return null;
        }

        private string GetMaDonVi(Guid? donvi_Id)
        {
            if (donvi_Id.HasValue)
            {
                var donvi = uow.DonVis.GetById(donvi_Id.Value);
                if (donvi != null)
                {
                    return donvi.MaDonVi;
                }
            }
            return null;
        }

        private string GetMaPhongBan(Guid? phongban_Id)
        {
            if (phongban_Id.HasValue)
            {
                var phongban = uow.phongbans.GetById(phongban_Id.Value);
                if (phongban != null)
                {
                    return phongban.MaPhongBan;
                }
            }
            return null;
        }

    }
}
