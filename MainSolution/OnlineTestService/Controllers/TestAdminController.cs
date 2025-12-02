using Microsoft.AspNetCore.Mvc;
using OnlineTestService.Dtos;
using OnlineTestService.Service;
using Swashbuckle.AspNetCore.Annotations;

namespace OnlineTestService.Controllers
{
    [ApiController]
    [Route("api/onlineTest/[controller]")]
    //[Authorize(Roles = "Admin")]
    [SwaggerTag("Quản lý đề thi (Dành cho Admin)")]
    public class TestAdminController : ControllerBase
    {
        private readonly ITestAdminService _adminService;
        private readonly IFileService _fileService;

        public TestAdminController(ITestAdminService adminService, IFileService fileService)
        {
            _adminService = adminService;
            _fileService = fileService;
        }

        [HttpPost("UploadAudio")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)] // Allow large files
        [DisableRequestSizeLimit] // Allow large files
        [SwaggerOperation(Summary = "Upload file âm thanh", Description = "Upload file MP3/WAV cho bài thi Listening. Trả về ID của file để gán vào bài thi.")]
        [SwaggerResponse(200, "Upload thành công. Trả về { audioFileId: int }", typeof(object))]
        [SwaggerResponse(400, "File không hợp lệ hoặc chưa chọn file")]
        [SwaggerResponse(500, "Lỗi server khi lưu file")]
        public async Task<IActionResult> UploadAudioFile(IFormFile audioFile) // Parameter name must match FormData key
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }

            try
            {
                int audioFileId = await _fileService.SaveAudioFileAsync(audioFile);
                // Return the ID of the newly created record
                return Ok(new { audioFileId = audioFileId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error uploading audio file: {ex.ToString()}");
                return StatusCode(500, "An error occurred while uploading the audio file.");
            }
        }

        [HttpGet("GetAllTestsForAdmin")]
        [SwaggerOperation(Summary = "Lấy danh sách quản lý đề thi", Description = "Danh sách dạng bảng, bao gồm ngày tạo, ngày sửa để Admin quản lý.")]
        [SwaggerResponse(200, "Danh sách đề thi", typeof(IEnumerable<AdminTestListItemDto>))]
        public async Task<IActionResult> GetAllTestsForAdmin()
        {
            var tests = await _adminService.GetAllTestsForAdminAsync();
            return Ok(tests);
        }

        [HttpPost("CreateTest")]
        [SwaggerOperation(Summary = "Tạo đề thi mới", Description = "Tạo mới một cấu trúc đề thi bao gồm các câu hỏi và settings.")]
        [SwaggerResponse(201, "Tạo thành công. Trả về ID bài thi mới.", typeof(object))]
        [SwaggerResponse(400, "Dữ liệu đầu vào không hợp lệ (thiếu trường bắt buộc)")]
        [SwaggerResponse(500, "Lỗi server khi lưu dữ liệu")]
        public async Task<IActionResult> CreateTest([FromBody] ManageTestDto createTestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var testId = await _adminService.CreateTestAsync(createTestDto);
            return CreatedAtAction(nameof(GetTestById), new { id = testId }, new { id = testId });
        }

        [HttpPut("UpdateTest/{id}")]
        [SwaggerOperation(Summary = "Cập nhật đề thi", Description = "Cập nhật toàn bộ nội dung đề thi theo ID.")]
        [SwaggerResponse(204, "Cập nhật thành công (Không trả về dữ liệu)")]
        [SwaggerResponse(400, "Dữ liệu gửi lên không hợp lệ")]
        [SwaggerResponse(404, "Không tìm thấy đề thi cần sửa")]
        public async Task<IActionResult> UpdateTest(int id, [FromBody] ManageTestDto updateTestDto)
        {
            var success = await _adminService.UpdateTestAsync(id, updateTestDto);
            if (!success) return NotFound($"Không tìm thấy bài test với ID = {id}");

            return NoContent();
        }

        [HttpDelete("DeleteTest/{id}")]
        [SwaggerOperation(Summary = "Xóa đề thi", Description = "Xóa vĩnh viễn đề thi khỏi hệ thống.")]
        [SwaggerResponse(204, "Xóa thành công")]
        [SwaggerResponse(404, "Không tìm thấy đề thi cần xóa")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var success = await _adminService.DeleteTestAsync(id);
            if (!success) return NotFound($"Không tìm thấy bài test với ID = {id}");

            return NoContent();
        }

        // Endpoint để lấy dữ liệu test cho việc sửa
        [HttpGet("GetTestById/{id}")]
        [SwaggerOperation(Summary = "Lấy dữ liệu đề thi để sửa", Description = "Trả về full cấu trúc đề thi để fill vào form chỉnh sửa.")]
        [SwaggerResponse(200, "Dữ liệu chi tiết đề thi", typeof(ManageTestDto))]
        [SwaggerResponse(404, "Không tìm thấy đề thi")]
        public async Task<IActionResult> GetTestById(int id)
        {
            var testData = await _adminService.GetTestForEditAsync(id);

            if (testData == null)
            {
                return NotFound($"Không tìm thấy bài test với ID = {id}");
            }

            return Ok(testData);
        }
        [HttpDelete("DeleteAudio/{audioFileId}")]
        [SwaggerOperation(Summary = "Xóa file âm thanh", Description = "Xóa file audio khỏi hệ thống (chỉ xóa được nếu chưa có bài test nào sử dụng).")]
        [SwaggerResponse(204, "Xóa thành công")]
        [SwaggerResponse(404, "File không tồn tại hoặc lỗi khi xóa")]
        [SwaggerResponse(500, "Lỗi server nội bộ")]
        public async Task<IActionResult> DeleteAudio(int audioFileId)
        {
            try
            {
                bool success = await _fileService.DeleteAudioFileAsync(audioFileId);
                if (!success)
                {
                    return NotFound($"Audio file with ID {audioFileId} not found or could not be deleted.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting audio file ID {audioFileId}: {ex.ToString()}");
                return StatusCode(500, "An error occurred while deleting the audio file.");
            }
        }
    }
}
