using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Domain.Gateway;
using Microsoft.Extensions.Configuration;

namespace Application.UseCases
{
    public class S3StorageAdapter : IStoragePort
    {
        private readonly string _SourceBucketName;
        private readonly string _DestinationBucketName;
        private readonly string _serviceUrl;
        private readonly string _region;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _downloadPath;

        public S3StorageAdapter(IConfiguration configuration)
        {
            _SourceBucketName = configuration["AWS:S3:SourceBucketName"];
            _DestinationBucketName = configuration["AWS:S3:DestinationBucketName"];
            _serviceUrl = configuration["AWS:S3:ServiceURL"];
            _region = configuration["AWS:S3:Region"];
            _accessKey = configuration["AWS:S3:AccessKey"];
            _secretKey = configuration["AWS:S3:SecretKey"];
        }

        public async Task DownloadVideoAsync(string videoId, string inputFilePath)
        {
            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);

            string keyName = videoId;

            var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                UseHttp = true,
                ForcePathStyle = true,
                AuthenticationRegion = _region,
            });

            var request = new GetObjectRequest
            {
                BucketName = _SourceBucketName,
                Key = keyName
            };

            using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
            {
                await response.WriteResponseStreamToFileAsync(inputFilePath, false, CancellationToken.None);
            }
        }

        public async Task UploadChunkAsync(string key, string filePath)
        {
            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);

            var s3Client = new AmazonS3Client(credentials, new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                UseHttp = true,
                ForcePathStyle = true,
                AuthenticationRegion = _region,
            });

            var fileTransferUtility = new TransferUtility(s3Client);

            await fileTransferUtility.UploadAsync(filePath, _DestinationBucketName, key);
        }
    }
}
