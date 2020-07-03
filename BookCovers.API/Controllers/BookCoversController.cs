using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookCovers.API.Controllers
{
    [Route("api/bookcovers")]
    [ApiController]
    public class BookCoversController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BookCoversController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetBookCover(
            string name,
            bool returnFault = false)
        {
            // if returnFault is true, wait 100ms and
            // return an Internal Server Error
            if (returnFault)
            {
                await Task.Delay(100);
                return new StatusCodeResult(500);
            }

            var webRootPath = _hostingEnvironment.WebRootPath;
            string filePath = Path.Combine($"{webRootPath}/images/amosMmmmm.jpg");
            var content = System.IO.File.ReadAllBytes(filePath);

            return Ok(new
            {
                Name = name,
                Content = content
            });
        }
    }
}
