using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SigneWordBotAspCore.Services;
using Telegram.Bot.Types;

namespace SigneWordBotAspCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly IUpdateService _updateService;

        public UpdateController(IUpdateService updateService)
        {
            _updateService = updateService;
        }

        // POST api/update
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            
            await _updateService.DoCommand(update);
//#if !DEBUG
////            await _updateService.EchoAsync(update);
//#endif
            return Ok();
        }
    }
}