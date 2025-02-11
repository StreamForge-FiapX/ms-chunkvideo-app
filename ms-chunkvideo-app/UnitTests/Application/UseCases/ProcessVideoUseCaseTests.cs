using Moq;
using Domain.Gateway;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Xabe.FFmpeg;
using Application.UseCases;

namespace Application.Tests
{
    public class ProcessVideoUseCaseTests
    {
        private readonly Mock<IChunkMetadataPort> _mockVideoPort;
        private readonly Mock<IMessageQueuePort> _mockQueuePort;
        private readonly Mock<IStoragePort> _mockStoragePort;
        private readonly Mock<IVideoProcessorPort> _mockVideoProcessorPort;
        private readonly Mock<IConfiguration> _mockConfiguration;

        private readonly ProcessVideoUseCase _useCase;

        public ProcessVideoUseCaseTests()
        {
            _mockVideoPort = new Mock<IChunkMetadataPort>();
            _mockQueuePort = new Mock<IMessageQueuePort>();
            _mockStoragePort = new Mock<IStoragePort>();
            _mockVideoProcessorPort = new Mock<IVideoProcessorPort>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(c => c["FilePath:DownloadPath"]).Returns("/path/to/download");
            _mockConfiguration.Setup(c => c["FilePath:ChunkPath"]).Returns("/path/to/chunks");
            _mockConfiguration.Setup(c => c["AWS:S3:DestinationBucketName"]).Returns("destination-bucket");

            _useCase = new ProcessVideoUseCase(
                _mockVideoPort.Object,
                _mockQueuePort.Object,
                _mockStoragePort.Object,
                _mockVideoProcessorPort.Object,
                _mockConfiguration.Object
            );
        }

        [Fact]
        public async Task Process_ShouldDownloadVideo_ChunkAndUpload()
        {
            // Arrange
            string videoName = "video.mp4";
            string inputFilePath = "/path/to/download/video.mp4";
            var chunk = new Chunk()
            {
                ChunkName = "video_part1.mp4",
                VideoName = videoName,
                DestinationBucket = "destination-bucket",
                TempFolderPath = "/path/to/chunks/video_part1.mp4"
            };

            _mockQueuePort.Setup(q => q.ConsumeMessage()).Returns(videoName);
            _mockStoragePort.Setup(s => s.DownloadVideoAsync(videoName, inputFilePath)).Returns(Task.CompletedTask);
            _mockVideoProcessorPort.Setup(v => v.GetMediaInfo(It.IsAny<string>())).ReturnsAsync(new Mock<IMediaInfo>().Object);
            _mockVideoProcessorPort.Setup(v => v.SaveFile(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockStoragePort.Setup(s => s.UploadChunkAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _mockVideoPort.Setup(v => v.SaveChunk(It.IsAny<Chunk>())).Verifiable();
            _mockQueuePort.Setup(q => q.PublishMessage(It.IsAny<string>())).Verifiable();

            // Act
            await _useCase.Process();

            // Assert
            _mockQueuePort.Verify(q => q.ConsumeMessage(), Times.Once);
            _mockStoragePort.Verify(s => s.DownloadVideoAsync(videoName, inputFilePath), Times.Once);
            _mockVideoProcessorPort.Verify(v => v.GetMediaInfo(inputFilePath), Times.Once);
            _mockVideoProcessorPort.Verify(v => v.SaveFile(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.AtLeastOnce);
            _mockStoragePort.Verify(s => s.UploadChunkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
            _mockVideoPort.Verify(v => v.SaveChunk(It.IsAny<Chunk>()), Times.AtLeastOnce);
            _mockQueuePort.Verify(q => q.PublishMessage(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Process_ShouldSkipWhenNoVideoNameIsConsumed()
        {
            // Arrange
            _mockQueuePort.Setup(q => q.ConsumeMessage()).Returns(string.Empty);

            // Act
            await _useCase.Process();

            // Assert
            _mockStoragePort.Verify(s => s.DownloadVideoAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockVideoProcessorPort.Verify(v => v.GetMediaInfo(It.IsAny<string>()), Times.Never);
            _mockStoragePort.Verify(s => s.UploadChunkAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockQueuePort.Verify(q => q.PublishMessage(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SplitIntoChunks_ShouldCreateChunksBasedOnVideoDuration()
        {
            // Arrange
            string videoId = "video.mp4";
            string inputFile = "/path/to/video.mp4";
            var mediaInfo = new Mock<IMediaInfo>();
            mediaInfo.Setup(m => m.Duration).Returns(TimeSpan.FromMinutes(2));

            _mockVideoProcessorPort.Setup(v => v.GetMediaInfo(It.IsAny<string>())).ReturnsAsync(mediaInfo.Object);
            _mockVideoProcessorPort.Setup(v => v.SaveFile(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<TimeSpan>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var chunks = await _useCase.SplitIntoChunks(videoId, inputFile);

            // Assert
            Assert.Equal(2, chunks.Count()); // 2 chunks for a 2-minute video with 1-minute parts
        }
    }
}
