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
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [SwaggerOperation(Summary = "Upload file âm thanh", Description = "Upload file MP3/WAV cho bài thi Listening. Trả về ID của file để gán vào bài thi.")]
        [SwaggerResponse(200, "Upload thành công", typeof(object))]
        [SwaggerResponse(400, "Lỗi file không hợp lệ", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server khi lưu file", typeof(ErrorResponse))]
        public async Task<IActionResult> UploadAudioFile(IFormFile audioFile)
        {
            try
            {
                if (audioFile == null || audioFile.Length == 0)
                {
                    return BadRequest(new ErrorResponse(400, "Chưa chọn file hoặc file rỗng."));
                }

                int audioFileId = await _fileService.SaveAudioFileAsync(audioFile);
                return Ok(new { audioFileId = audioFileId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse(400, ex.Message));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading audio file: {ex}");
                return StatusCode(500, new ErrorResponse(500, "Lỗi server khi upload file: " + ex.Message));
            }
        }

        [HttpGet("GetAllTestsForAdmin")]
        [SwaggerOperation(Summary = "Lấy danh sách quản lý đề thi")]
        [SwaggerResponse(200, "Danh sách đề thi", typeof(IEnumerable<AdminTestListItemDto>))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> GetAllTestsForAdmin()
        {
            try
            {
                var tests = await _adminService.GetAllTestsForAdminAsync();
                return Ok(tests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi lấy danh sách: " + ex.Message));
            }
        }

        [HttpPost("CreateTest")]
        [SwaggerOperation(Summary = "Tạo đề thi mới", Description = "Tạo mới một cấu trúc đề thi bao gồm các câu hỏi và đáp án.")]
        [SwaggerResponse(201, "Tạo thành công", typeof(object))]
        [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(object))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> CreateTest([FromBody] ManageTestDto createTestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var testId = await _adminService.CreateTestAsync(createTestDto);
                return CreatedAtAction(nameof(GetTestById), new { id = testId }, new { id = testId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi tạo đề thi: " + ex.Message));
            }
        }

        [HttpPut("UpdateTest/{id}")]
        [SwaggerOperation(Summary = "Cập nhật đề thi", Description = "Cập nhật nội dung đề thi theo ID.")]
        [SwaggerResponse(204, "Cập nhật thành công")]
        [SwaggerResponse(400, "Dữ liệu không hợp lệ", typeof(object))]
        [SwaggerResponse(404, "Không tìm thấy đề thi", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> UpdateTest(int id, [FromBody] ManageTestDto updateTestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var success = await _adminService.UpdateTestAsync(id, updateTestDto);
                if (!success)
                {
                    return NotFound(new ErrorResponse(404, $"Không tìm thấy bài test với ID = {id} để cập nhật."));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi cập nhật đề thi: " + ex.Message));
            }
        }

        [HttpDelete("DeleteTest/{id}")]
        [SwaggerOperation(Summary = "Xóa đề thi", Description = "Xóa đề thi khỏi dữ liệu hệ thống.")]
        [SwaggerResponse(204, "Xóa thành công")]
        [SwaggerResponse(404, "Không tìm thấy đề thi", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteTest(int id)
        {
            try
            {
                var success = await _adminService.DeleteTestAsync(id);
                if (!success)
                {
                    return NotFound(new ErrorResponse(404, $"Không tìm thấy bài test với ID = {id} để xóa."));
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi xóa đề thi: " + ex.Message));
            }
        }

        // Endpoint để lấy dữ liệu test cho việc sửa
        [HttpGet("GetTestById/{id}")]
        [SwaggerOperation(Summary = "Lấy dữ liệu đề thi để sửa", Description = "Trả về full cấu trúc đề thi.")]
        [SwaggerResponse(200, "Dữ liệu chi tiết", typeof(ManageTestDto))]
        [SwaggerResponse(404, "Không tìm thấy đề thi", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> GetTestById(int id)
        {
            try
            {
                var testData = await _adminService.GetTestForEditAsync(id);

                if (testData == null)
                {
                    return NotFound(new ErrorResponse(404, $"Không tìm thấy bài test với ID = {id}"));
                }

                return Ok(testData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi server: " + ex.Message));
            }
        }

        [HttpDelete("DeleteAudio/{audioFileId}")]
        [SwaggerOperation(Summary = "Xóa file âm thanh", Description = "Xóa file audio khỏi hệ thống (chỉ xóa được nếu không có bài test nào sử dụng).")]
        [SwaggerResponse(204, "Xóa thành công")]
        [SwaggerResponse(404, "File không tồn tại hoặc lỗi xóa", typeof(ErrorResponse))]
        [SwaggerResponse(500, "Lỗi server", typeof(ErrorResponse))]
        public async Task<IActionResult> DeleteAudio(int audioFileId)
        {
            try
            {
                bool success = await _fileService.DeleteAudioFileAsync(audioFileId);
                if (!success)
                {
                    return NotFound(new ErrorResponse(404, $"Audio file ID {audioFileId} không tồn tại hoặc không thể xóa (đang được sử dụng)."));
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting audio file ID {audioFileId}: {ex}");
                return StatusCode(500, new ErrorResponse(500, "Lỗi server khi xóa file audio: " + ex.Message));
            }
        }
    }
}
