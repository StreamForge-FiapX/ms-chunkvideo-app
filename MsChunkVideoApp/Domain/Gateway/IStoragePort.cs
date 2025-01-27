using Domain.Entities;

namespace Application.UseCases
{
    public interface IStoragePort
    {
        public Task<Stream> DownloadVideoAsync(string key, string videoId);
        public Task UploadChunkAsync(string key, Stream inputStream);        
    }
}
