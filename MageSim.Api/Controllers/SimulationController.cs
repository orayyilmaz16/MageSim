using MageSim.Application.Simulation;
using Microsoft.AspNetCore.Mvc;

namespace MageSim.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly Coordinator _coord;

        public SimulationController(Coordinator coord)
        {
            _coord = coord;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            await _coord.StartAllAsync();
            return Ok("Simulation started");
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            _coord.StopAll();
            return Ok("Simulation stopped");
        }

        [HttpGet("clients")]
        public IActionResult GetClients()
        {
            var clients = _coord.Clients.Select(c => c.Id);
            return Ok(clients);
        }
    }
}
