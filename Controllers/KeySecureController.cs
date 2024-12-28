using ERP.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
  [EnableCors("CorsApi")]
  [AllowAnonymous]
  [Route("api/[controller]")]
  [ApiController]
  public class KeySecureController : ControllerBase
  {
    public KeySecureController()
    {
    }
    [HttpGet]
    public ActionResult Get()
    {
      return Ok("abc");
    }

  }
}