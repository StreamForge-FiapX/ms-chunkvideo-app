using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace Application.UseCases
{
    public class StorageAdapter : IStoragePort
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _SourceBucketName;
        private readonly string _DestinationBucketName;        

        public StorageAdapter(IConfiguration configuration)
        {
            _s3Client = new AmazonS3Client();
            _SourceBucketName = configuration["AWS:S3:SourceBucketName"];
            _DestinationBucketName = configuration["AWS:S3:DestinationBucketName"];
        }

        public async Task<Stream> DownloadVideoAsync(string key, string videoId)
        {
            var request = new GetObjectRequest
            {
                BucketName = _SourceBucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;
        }

        public async Task UploadChunkAsync(string key, Stream inputStream)
        {
            var request = new PutObjectRequest
            {
                BucketName = _DestinationBucketName,
                Key = key,
                InputStream = inputStream
            };

            await _s3Client.PutObjectAsync(request);
        }
    }
}
