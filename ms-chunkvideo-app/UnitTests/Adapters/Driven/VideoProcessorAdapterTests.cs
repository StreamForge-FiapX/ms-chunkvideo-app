using Moq;
using Xunit;
using Infra;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Infra.Tests
{
    public class VideoProcessorAdapterTests
    {
        private readonly Mock<IFFmpegWrapper> _mockFfmpegWrapper;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly VideoProcessorAdapter _videoProcessorAdapter;

        public VideoProcessorAdapterTests()
        {
            _mockFfmpegWrapper = new Mock<IFFmpegWrapper>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["FFmpeg:ExecutablePath"]).Returns("/path/to/ffmpeg");

            _videoProcessorAdapter = new VideoProcessorAdapter(_mockConfiguration.Object, _mockFfmpegWrapper.Object);
        }

        [Fact]
        public async Task SaveFile_ShouldCallFFmpegWithCorrectParameters()
        {
            // Arrange
            string inputFile = "/path/to/input/video.mp4";
            string outputFilePath = "/path/to/output/video_part.mp4";
            TimeSpan startTime = TimeSpan.FromMinutes(1);
            TimeSpan partDuration = TimeSpan.FromMinutes(2);

            // Act
            await _videoProcessorAdapter.SaveFile(inputFile, startTime, partDuration, outputFilePath);

            // Assert
            _mockFfmpegWrapper.Verify(ffmpeg => ffmpeg.StartConversion(
                It.Is<string>(s => s == inputFile),
                It.Is<TimeSpan>(t => t == startTime),
                It.Is<TimeSpan>(t => t == partDuration),
                It.Is<string>(s => s == outputFilePath)),
                Times.Once);
        }

        [Fact]
        public async Task GetMediaInfo_ShouldReturnMediaInfo()
        {
            // Arrange
            string inputFile = "/path/to/input/video.mp4";
            var mockMediaInfo = new Mock<IMediaInfo>();
            _mockFfmpegWrapper.Setup(ffmpeg => ffmpeg.GetMediaInfo(It.IsAny<string>())).ReturnsAsync(mockMediaInfo.Object);

            // Act
            var result = await _videoProcessorAdapter.GetMediaInfo(inputFile);

            // Assert
            Assert.Equal(mockMediaInfo.Object, result);
            _mockFfmpegWrapper.Verify(ffmpeg => ffmpeg.GetMediaInfo(It.Is<string>(s => s == inputFile)), Times.Once);
        }
    }
}
