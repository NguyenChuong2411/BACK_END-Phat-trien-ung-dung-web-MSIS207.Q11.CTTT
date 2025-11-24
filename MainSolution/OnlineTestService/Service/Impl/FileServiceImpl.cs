using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using ModelClass.OnlineTest;

namespace OnlineTestService.Service.Impl
{
    public class FileServiceImpl : IFileService
    {
        private readonly OnlineTestDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment; // Inject this

        public FileServiceImpl(OnlineTestDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<int> SaveAudioFileAsync(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new ArgumentException("No audio file provided.");
            }

            // 1. Define the storage path relative to wwwroot or ContentRoot
            // We store the *relative* path in the DB (/storage/Audio/...)
            // The absolute path is used for saving only.
            string relativeDirPath = Path.Combine("Storage", "Audio");
            string absoluteDirPath = Path.Combine(_webHostEnvironment.ContentRootPath, relativeDirPath); // Use ContentRootPath if Storage is outside wwwroot

            // Create directory if it doesn't exist
            if (!Directory.Exists(absoluteDirPath))
            {
                Directory.CreateDirectory(absoluteDirPath);
            }

            // 2. Generate a unique file name to prevent overwrites
            string uniqueFileName = $"{Path.GetFileNameWithoutExtension(audioFile.FileName)}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{Path.GetExtension(audioFile.FileName)}";
            string absoluteFilePath = Path.Combine(absoluteDirPath, uniqueFileName);
            // Construct the path to be saved in the DB (relative to the base URL)
            string storagePath = $"/{relativeDirPath.Replace("\\", "/")}/{uniqueFileName}"; // Use forward slashes for URL


            // 3. Save the file to the absolute path
            using (var stream = new FileStream(absoluteFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // 4. Create database record
            var audioFileRecord = new AudioFile
            {
                FileName = audioFile.FileName, // Original file name
                StoragePath = storagePath,     // Relative path for URL access
                UploadedAt = DateTime.UtcNow
                // DurationSeconds can be added later if needed (requires library)
            };

            _context.AudioFiles.Add(audioFileRecord);
            await _context.SaveChangesAsync();

            // 5. Return the ID
            return audioFileRecord.Id;
        }
        public async Task<bool> DeleteAudioFileAsync(int audioFileId)
        {
            var audioFileRecord = await _context.AudioFiles.FindAsync(audioFileId);
            if (audioFileRecord == null)
            {
                return false; // Không tìm thấy file để xóa
            }

            // Kiểm tra xem file này có đang được sử dụng bởi BẤT KỲ bài test nào không
            bool isUsedByAnyTest = await _context.Tests.AnyAsync(t => t.AudioFileId == audioFileId);

            // Chỉ xóa file vật lý nếu không còn bài test nào sử dụng
            if (!isUsedByAnyTest)
            {
                try
                {
                    // Lấy đường dẫn tuyệt đối
                    // Lưu ý: Cần đảm bảo logic lấy đường dẫn này khớp với logic trong SaveAudioFileAsync
                    string relativePathFromUrl = audioFileRecord.StoragePath.TrimStart('/'); // Bỏ dấu / ở đầu nếu có
                    string absoluteFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, relativePathFromUrl.Replace("/", "\\")); // Thay / thành \ cho Windows

                    if (File.Exists(absoluteFilePath))
                    {
                        File.Delete(absoluteFilePath);
                        Console.WriteLine($"Deleted physical file: {absoluteFilePath}");
                    }
                    else
                    {
                        Console.WriteLine($"Physical file not found, skipping delete: {absoluteFilePath}");
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi xóa file vật lý nhưng vẫn tiếp tục xóa record DB
                    Console.WriteLine($"Error deleting physical audio file ID {audioFileId}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Audio file ID {audioFileId} is still in use by other tests. Skipping physical file deletion.");
            }

            // Xóa bản ghi khỏi database BẤT KỂ file vật lý có được xóa hay không
            // (Nếu file vẫn được dùng, bản ghi này sẽ không được xóa do ràng buộc FK, điều này là đúng)
            try
            {
                _context.AudioFiles.Remove(audioFileRecord);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Deleted AudioFile record ID {audioFileId} from database.");
                return true; // Xóa record DB thành công (hoặc không cần xóa vì file vật lý vẫn dùng)
            }
            catch (DbUpdateException ex) // Bắt lỗi nếu không xóa được do FK
            {
                Console.WriteLine($"Could not delete AudioFile record ID {audioFileId} (likely due to FK constraint): {ex.Message}");
                // Trả về true vì mục đích là gỡ liên kết khỏi bài test hiện tại đã xong ở bước UpdateTestAsync
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting AudioFile record ID {audioFileId} from database: {ex.Message}");
                return false; // Lỗi khác khi xóa record DB
            }
        }
    }
}
