// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.SignalR;
// using ERP.Controllers;

// namespace ERP.HubConfig
// {
//  [Authorize]
//   public class XeRaCongHub : Hub
//   {
//     private readonly KhoThanhPhamController xeracongController;
//     public XeRaCongHub(KhoThanhPhamController _xeracongController)
//     {
//       xeracongController = _xeracongController;
//     }
//    public async Task SendThongTinXeRaCong(object xeData)
//     {
//       await Clients.All.SendAsync("ReceiveXeData", xeData);
//     }
//   }


// }