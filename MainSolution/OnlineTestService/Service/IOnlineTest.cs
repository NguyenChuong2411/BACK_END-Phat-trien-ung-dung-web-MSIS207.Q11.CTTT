using OnlineTestService.Dtos;

namespace OnlineTestService.Service
{
    public interface IOnlineTest
    {
        Task<IEnumerable<TestListItemDto>> GetAllTestsAsync();
        Task<FullTestDto?> GetTestDetailsByIdAsync(int testId);
        Task<ListeningTestDto?> GetListeningTestDetailsByIdAsync(int testId);
        Task<int> SubmitTestAsync(TestSubmissionDto submission);
        Task<TestResultDto?> GetTestResultAsync(int attemptId);
    }
}
