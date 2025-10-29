using Microsoft.AspNetCore.Mvc;
using OnlineTestService.Dtos;
using OnlineTestService.Service;

namespace OnlineTestService.Controllers
{
    [ApiController]
    [Route("api/onlineTest/[controller]")]
    //[Authorize(Roles = "Admin")]
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
        public async Task<IActionResult> GetAllTestsForAdmin()
        {
            var tests = await _adminService.GetAllTestsForAdminAsync();
            return Ok(tests);
        }

        [HttpPost("CreateTest")]
        public async Task<IActionResult> CreateTest([FromBody] ManageTestDto createTestDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var testId = await _adminService.CreateTestAsync(createTestDto);
            return CreatedAtAction(nameof(GetTestById), new { id = testId }, new { id = testId });
        }

        [HttpPut("UpdateTest/{id}")]
        public async Task<IActionResult> UpdateTest(int id, [FromBody] ManageTestDto updateTestDto)
        {
            var success = await _adminService.UpdateTestAsync(id, updateTestDto);
            if (!success) return NotFound($"Không tìm thấy bài test với ID = {id}");

            return NoContent();
        }

        [HttpDelete("DeleteTest/{id}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            var success = await _adminService.DeleteTestAsync(id);
            if (!success) return NotFound($"Không tìm thấy bài test với ID = {id}");

            return NoContent();
        }

        // Endpoint để lấy dữ liệu test cho việc sửa
        [HttpGet("GetTestById/{id}")]
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
