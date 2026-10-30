using CuestionarioNom035.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NOM35.Web.Data;

namespace CuestionarioNom035.Controllers
{
    [Route("api/areas")]
    [ApiController]
    [AllowAnonymous] // lo usa el inicio de la Guía III
    public class ApiAreasController : ControllerBase
    {
        [HttpGet("subareas")]
        public IActionResult GetSubareas([FromQuery] string area)
        {
            var sub = CatalogoAreas.Subareas(area);
            return Ok(sub);
        }
    }
}
