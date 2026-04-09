using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}