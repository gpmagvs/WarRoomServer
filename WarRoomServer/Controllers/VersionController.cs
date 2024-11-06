using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarRoomServer.Services;

namespace WarRoomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private readonly VersionService _versionService;
        public VersionController(VersionService versionService)
        {
            _versionService = versionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVersions()
        {
            List<View.Version.AGVSFieldVersionInfo> result = await _versionService.GetAllVersions();
            return Ok(result);
        }
    }
}
