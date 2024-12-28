using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Controllers
{
  [EnableCors("CorsApi")]
  [Authorize]
  [Route("api/[controller]")]
  [ApiController]

  public class LogController : ControllerBase

  {
    private readonly IUnitofWork uow;
    public LogController(IUnitofWork _uow)
    {
      uow = _uow;
    }
    [HttpGet]
    public ActionResult Get(DateTime? TuNgay = null, DateTime? DenNgay = null, int page = 1, int pageSize = 20, string keyword = null)
    {
      if (keyword == null || keyword == "") return Ok();
      if (TuNgay == null)
      {
        TuNgay = new DateTime(2020, 1, 1);
      }
      if (DenNgay == null)
      {
        DenNgay = DateTime.Now;
      }
      Expression<Func<Log, bool>> whereFunc = item => (item.ApplicationUser.UserName.Contains(keyword.ToLower())
      || item.ApplicationUser.Email.Contains(keyword.ToLower()) || item.ApplicationUser.FullName.Contains(keyword.ToLower()
      ) && (EF.Functions.DateDiffDay(TuNgay, item.AccessDate) >= 0
      && EF.Functions.DateDiffDay(item.AccessDate, DenNgay) >= 0));
      Func<IQueryable<Log>, IOrderedQueryable<Log>> orderByFunc = item => item.OrderByDescending(x => x.AccessDate);
      var lst_root = uow.Logs.GetAll(whereFunc, orderByFunc).Select(x => new { Id = x.Id }).ToList();
      int totalRow = lst_root.Count();
      int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);
      var lst_id = lst_root.Select(a => a.Id).Skip((page - 1) * pageSize).Take(pageSize).ToList();
      Expression<Func<Log, bool>> whereFunc1 = null;
      if (lst_id.Count() > 0)
      {
        whereFunc1 = item => lst_id.Contains(item.Id);
      }
      else whereFunc1 = whereFunc;
      string[] includes = { "ApplicationUser" };
      var result = uow.Logs.GetAll(whereFunc1, orderByFunc, includes).Select(x => new { x.Id, x.Url, x.Data, x.AccessdBy, x.AccessDate, x.IpAddress, x.Type, x.ApplicationUser.UserName, x.ApplicationUser.Email, x.ApplicationUser.FullName }).ToList();
      return Ok(new
      {
        totalRow,
        totalPage,
        result
      });
    }
  }
}