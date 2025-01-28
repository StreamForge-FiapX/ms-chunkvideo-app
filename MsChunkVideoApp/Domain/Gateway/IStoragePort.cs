using Domain.Entities;

namespace Application.UseCases
{
    public interface IStoragePort
    {
        public Task DownloadVideoAsync(string key, string videoId);
        public Task UploadChunkAsync(string key, string filePath);
    }
}
