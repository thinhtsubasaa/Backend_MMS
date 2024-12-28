using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using ERP.Controllers;
using ERP.HubConfig;
using ERP.Infrastructure;
using ERP.Models;
using Newtonsoft.Json;

namespace ERP.Filters
{
  public class UserFilterAttribute : IActionFilter
  {
    private readonly IUnitofWork uow;
    private IHubContext<DashboardHub> hub;
    private readonly DashboardController dashboardController;
    public UserFilterAttribute(IUnitofWork _uow, IHubContext<DashboardHub> _hub, DashboardController _dashboardController)
    {
      uow = _uow;
      hub = _hub;
       dashboardController = _dashboardController;
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
    public void OnActionExecuting(ActionExecutingContext context)
    {
      string data = "";
      var routeData = context.RouteData;
      var controller = routeData.Values["controller"];
      var action = routeData.Values["action"];
      var url = $"{controller}/{action}";
      var method = context.HttpContext.Request.Method.ToString();
      int Type_Login_ChangePass = 0;
      if (!string.IsNullOrEmpty(context.HttpContext.Request.QueryString.Value))
      {
        data = context.HttpContext.Request.QueryString.Value;
      }
      else
      {
        var arguments = context.ActionArguments;
        var value = arguments.FirstOrDefault().Value;
        if (action.ToString() == "Authencation")
        {
          Type_Login_ChangePass = 1;
        }
        if (action.ToString() == "ChangePassword")
        {
          Type_Login_ChangePass = 2;
        }
        var convertedValue = JsonConvert.SerializeObject(value);
        data = convertedValue;
      }
      var user = context.HttpContext.User.Identity.Name;
      var ipAddress = context.HttpContext.Connection.LocalIpAddress.ToString();
      if (method != "GET" && controller.ToString() !="KeySercure")
        SaveUserActivity(data, url,controller.ToString(), method, user, ipAddress, Type_Login_ChangePass);
    }
    public void SaveUserActivity(string data, string url, string controller, string type, string user, string ipAddress, int Type_Login_ChangePass)
    {
      if (Type_Login_ChangePass == 1)
      {
        dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
        jsonObject.Password = "*****";
        data = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
      }
      if (Type_Login_ChangePass == 2)
      {
        dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
        jsonObject.Password = "*****";
        jsonObject.NewPassword = "*****";
        jsonObject.ConfirmNewPassword = "*****";
        data = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject);
      }
      Guid? us = (Guid?)null;
      if (user != null)
      {
        us = Guid.Parse(user);
      }
      var userActivity = new Log
      {
        Data = data,
        Type = type,
        Url = url,
        AccessdBy = us,
        IpAddress = ipAddress,
        AccessDate = DateTime.Now
      };
      uow.Logs.Add(userActivity);
      uow.Complete();
      //Điều kiện để trả về key dữ liệu Realtime
      // if(user != null && controller =="DiemDuaDon" && (type =="POST" || type =="PUT") )
      // hub.Clients.All.SendAsync("Receive_Dashboard", dashboardController.Get(user));
    }
  }
}