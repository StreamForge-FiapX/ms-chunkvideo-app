using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MsChunkVideoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoProcessingController : ControllerBase
    {
        private IProcessVideoUseCase _processVideoUseCase;
        public VideoProcessingController(IProcessVideoUseCase processVideoUseCase) { 
            _processVideoUseCase = processVideoUseCase;
        }

        // GET: api/<VideoController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            await _processVideoUseCase.Process();

            return Ok();
        }

        // POST api/<VideoController>
        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            await _processVideoUseCase.Process();

            return Ok();
        }

        // PUT api/<VideoController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }        
    }
}
