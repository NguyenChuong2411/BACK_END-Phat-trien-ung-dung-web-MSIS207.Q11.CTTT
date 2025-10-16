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

        public TestAdminController(ITestAdminService adminService)
        {
            _adminService = adminService;
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
    }
}
