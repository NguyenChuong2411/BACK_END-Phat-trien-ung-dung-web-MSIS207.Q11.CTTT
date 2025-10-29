namespace OnlineTestService.Service
{
    public interface IFileService
    {
        Task<int> SaveAudioFileAsync(IFormFile audioFile);
        Task<bool> DeleteAudioFileAsync(int audioFileId);
    }
}
