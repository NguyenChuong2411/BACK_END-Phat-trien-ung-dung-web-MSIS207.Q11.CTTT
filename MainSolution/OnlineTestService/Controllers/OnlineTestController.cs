using Microsoft.AspNetCore.Mvc;
using OnlineTestService.Service;

namespace OnlineTestService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineTestController : ControllerBase
    {
        private readonly IOnlineTest _onlineTestService;

        public OnlineTestController(IOnlineTest onlineTestService)
        {
            _onlineTestService = onlineTestService;
        }

        // GET: /api/tests
        // Lấy danh sách tất cả bài test
        [HttpGet("GetAllTests")]
        public async Task<IActionResult> GetAllTests()
        {
            var tests = await _onlineTestService.GetAllTestsAsync();
            return Ok(tests);
        }

        // GET: /api/tests/1
        // Lấy chi tiết một bài test theo id
        [HttpGet("GetTestDetails/{id}")]
        public async Task<IActionResult> GetTestDetails(int id)
        {
            var testDetails = await _onlineTestService.GetTestDetailsByIdAsync(id);
            if (testDetails == null)
            {
                return NotFound($"Không tìm thấy bài test với ID = {id}");
            }
            return Ok(testDetails);
        }
    }
}
