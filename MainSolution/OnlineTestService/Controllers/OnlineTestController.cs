using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTestService.Dtos;
using OnlineTestService.Service;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace OnlineTestService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Quản lý các bài thi và nộp bài")]
    public class OnlineTestController : ControllerBase
    {
        private readonly IOnlineTest _onlineTestService;

        public OnlineTestController(IOnlineTest onlineTestService)
        {
            _onlineTestService = onlineTestService;
        }

        // Lấy danh sách tất cả bài test
        [HttpGet("GetAllTests")]
        [SwaggerOperation(Summary = "Lấy danh sách tất cả bài test", Description = "Trả về danh sách tóm tắt các bài thi hiện có.")]
        [SwaggerResponse(200, "Danh sách bài thi lấy thành công", typeof(IEnumerable<TestListItemDto>))]
        public async Task<IActionResult> GetAllTests()
        {
            var tests = await _onlineTestService.GetAllTestsAsync();
            return Ok(tests);
        }

        // Lấy chi tiết một bài test theo id
        [HttpGet("GetTestDetails/{id}")]
        [SwaggerOperation(Summary = "Lấy chi tiết đề thi", Description = "Bao gồm các đoạn văn, câu hỏi Reading, Speaking, Writing.")]
        [SwaggerResponse(200, "Chi tiết đề thi", typeof(FullTestDto))]
        [SwaggerResponse(404, "Không tìm thấy đề thi với ID này")]
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
        [SwaggerOperation(Summary = "Lấy chi tiết đề thi Nghe", Description = "Bao gồm đường dẫn file audio và các câu hỏi.")]
        [SwaggerResponse(200, "Chi tiết đề thi Listening", typeof(ListeningTestDto))]
        [SwaggerResponse(404, "Không tìm thấy đề thi hoặc file Audio")]
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
        [SwaggerOperation(Summary = "Nộp bài thi", Description = "Chấm điểm tự động và lưu kết quả.")]
        [SwaggerResponse(200, "Nộp bài thành công. Trả về AttemptId (ID lượt thi).", typeof(object))]
        [SwaggerResponse(401, "Người dùng chưa đăng nhập")]
        [SwaggerResponse(500, "Lỗi server trong quá trình chấm điểm")]
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
        [SwaggerOperation(Summary = "Xem kết quả bài thi", Description = "Xem lại điểm số và đáp án chi tiết của một lượt thi.")]
        [SwaggerResponse(200, "Kết quả bài thi", typeof(TestResultDto))]
        [SwaggerResponse(404, "Không tìm thấy kết quả")]
        public async Task<IActionResult> GetResult(int attemptId)
        {
            var result = await _onlineTestService.GetTestResultAsync(attemptId);
            if (result == null)
            {
                return NotFound("Không tìm thấy kết quả làm bài.");
            }
            return Ok(result);
        }
        [HttpGet("GetMyTestHistory")]
        [Authorize]
        [SwaggerOperation(Summary = "Lịch sử thi của tôi", Description = "Lấy danh sách các bài đã thi của người dùng hiện tại.")]
        [SwaggerResponse(200, "Danh sách lịch sử thi", typeof(IEnumerable<TestAttemptHistoryDto>))]
        [SwaggerResponse(401, "Chưa đăng nhập")]
        public async Task<IActionResult> GetMyTestHistory()
        {
            try
            {
                var history = await _onlineTestService.GetMyTestHistoryAsync();
                return Ok(history);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }
    }
}
