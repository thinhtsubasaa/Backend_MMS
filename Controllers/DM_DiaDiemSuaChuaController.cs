using ERP.Infrastructure;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using static ERP.Data.MyDbContext;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Presentation;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using System.Linq.Expressions;
using ERP.Helpers;
using System.Threading.Tasks;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_DM_SuaChuaController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        public MMS_DM_SuaChuaController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, DataService master)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
        }

        [HttpGet()]
        public async Task<ActionResult> GetDataFromDb()
        {
            var dataList = await _master.GetDiadiem();
            var data = dataList.Where(x => !x.IsDeleted && x.DiaLy_LoaiDiaDiem_Id == Guid.Parse("45a7f16d-d1ac-4ad3-811b-9cce4eb82973"));
            return Ok(data);
        }
    }
}
