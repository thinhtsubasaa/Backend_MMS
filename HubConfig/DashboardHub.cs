using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ERP.Controllers;

namespace ERP.HubConfig
{
 [Authorize]
  public class DashboardHub : Hub
  {
    private readonly DashboardController dashboardController;
    public DashboardHub(DashboardController _dashboardController)
    {
      dashboardController = _dashboardController;
    }
    public override Task OnConnectedAsync()
    {
      string name = Context.User.Identity.Name;
      Clients.All.SendAsync("Receive_Dashboard", dashboardController.Get(name));
      return base.OnConnectedAsync();
    }
  }
}