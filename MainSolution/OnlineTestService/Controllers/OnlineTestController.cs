using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTestService.Dtos;
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

        // Lấy danh sách tất cả bài test
        [HttpGet("GetAllTests")]
        public async Task<IActionResult> GetAllTests()
        {
            var tests = await _onlineTestService.GetAllTestsAsync();
            return Ok(tests);
        }

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
        // Lấy chi tiết bài thi listening
        [HttpGet("GetListeningTestDetails/{id}")]
        public async Task<IActionResult> GetListeningTestDetails(int id)
        {
            var testDetails = await _onlineTestService.GetListeningTestDetailsByIdAsync(id);
            if (testDetails == null)
            {
                return NotFound($"Không tìm thấy bài thi Listening với ID = {id}");
            }
            return Ok(testDetails);
        }
        [HttpPost("Submit")]
        [Authorize]
        public async Task<IActionResult> SubmitTest([FromBody] TestSubmissionDto submission)
        {
            try
            {
                var attemptId = await _onlineTestService.SubmitTestAsync(submission);
                return Ok(new { AttemptId = attemptId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi nộp bài.");
            }
        }
        [HttpGet("Result/{attemptId}")]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var result = await _onlineTestService.GetTestResultAsync(attemptId);
            if (result == null)
            {
                return NotFound("Không tìm thấy kết quả làm bài.");
            }
            return Ok(result);
        }
    }
}
