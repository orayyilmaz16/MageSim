using MageSim.Infrastructure.Config;
using Microsoft.AspNetCore.Mvc;

namespace MageSim.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly ConfigService _config;

        public ConfigController(ConfigService config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var root = await _config.LoadAsync();
            return Ok(root);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] RootConfig cfg)
        {
            await _config.SaveAsync(cfg);
            return Ok("Config saved");
        }
    }
}
