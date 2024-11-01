using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WarRoomServer.Services;

namespace WarRoomServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FieldInfoController : ControllerBase
    {
        DataCacheService _dataCacheService;
        public FieldInfoController(DataCacheService dataCacheService)
        {
            _dataCacheService = dataCacheService;
        }

        [HttpGet("FieldsInfo")]
        public async Task<IActionResult> FieldsInfo()
        {

            List<View.FieldInfoView> fieldsInfo = await _dataCacheService.GetFieldsInfoOrUpdate();

            return Ok(fieldsInfo);
        }
    }
}
