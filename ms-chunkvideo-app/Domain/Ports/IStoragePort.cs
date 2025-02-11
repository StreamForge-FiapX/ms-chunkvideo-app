namespace Domain.Gateway
{
    public interface IStoragePort
    {
        Task DownloadVideoAsync(string key, string videoId);
        Task UploadChunkAsync(string key, string filePath);
    }
}
