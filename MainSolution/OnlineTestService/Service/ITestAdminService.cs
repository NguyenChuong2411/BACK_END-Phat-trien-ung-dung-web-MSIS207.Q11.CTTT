using OnlineTestService.Dtos;

namespace OnlineTestService.Service
{
    public interface ITestAdminService
    {
        Task<int> CreateTestAsync(ManageTestDto createTestDto);
        Task<bool> UpdateTestAsync(int testId, ManageTestDto updateTestDto);
        Task<bool> DeleteTestAsync(int testId);
        Task<IEnumerable<AdminTestListItemDto>> GetAllTestsForAdminAsync();
        Task<ManageTestDto?> GetTestForEditAsync(int testId);
    }
}
