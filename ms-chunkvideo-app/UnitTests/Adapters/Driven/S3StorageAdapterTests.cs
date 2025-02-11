using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3;
using Application.UseCases;
using Moq;
using Microsoft.Extensions.Configuration;

namespace UnitTests.Adapters.Driven
{
    public class S3StorageAdapterTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Mock<ITransferUtility> _mockTransferUtility;
        private readonly S3StorageAdapter _s3StorageAdapter;

        public S3StorageAdapterTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["AWS:S3:SourceBucketName"]).Returns("sourceBucket");
            _mockConfiguration.Setup(c => c["AWS:S3:DestinationBucketName"]).Returns("destinationBucket");
            _mockConfiguration.Setup(c => c["AWS:S3:ServiceURL"]).Returns("http://localhost:9000");
            _mockConfiguration.Setup(c => c["AWS:S3:Region"]).Returns("us-west-1");
            _mockConfiguration.Setup(c => c["AWS:S3:AccessKey"]).Returns("accessKey");
            _mockConfiguration.Setup(c => c["AWS:S3:SecretKey"]).Returns("secretKey");

            _mockS3Client = new Mock<IAmazonS3>();
            _mockTransferUtility = new Mock<ITransferUtility>();

            _s3StorageAdapter = new S3StorageAdapter(_mockConfiguration.Object);
        }

        [Fact]
        public async Task DownloadVideoAsync_ShouldDownloadFileSuccessfully()
        {
            // Arrange
            string videoId = "testVideoId";
            string inputFilePath = "/path/to/file";

            // Mock GetObjectAsync response
            var mockGetObjectResponse = new Mock<GetObjectResponse>();
            _mockS3Client.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default)).ReturnsAsync(mockGetObjectResponse.Object);

            // Act
            await _s3StorageAdapter.DownloadVideoAsync(videoId, inputFilePath);

            // Assert
            _mockS3Client.Verify(x => x.GetObjectAsync(It.Is<GetObjectRequest>(req => req.BucketName == "sourceBucket" && req.Key == videoId), default), Times.Once);
            // Here you can also assert other things, e.g., if the file write to disk was attempted
        }

        [Fact]
        public async Task UploadChunkAsync_ShouldUploadFileSuccessfully()
        {
            // Arrange
            string key = "testKey";
            string filePath = "/path/to/file";

            // Act
            await _s3StorageAdapter.UploadChunkAsync(key, filePath);

            // Assert
            _mockTransferUtility.Verify(x => x.Upload(It.Is<string>(path => path == filePath), It.Is<string>(bucket => bucket == "destinationBucket"), It.Is<string>(k => k == key)), Times.Once);
        }
    }
}

