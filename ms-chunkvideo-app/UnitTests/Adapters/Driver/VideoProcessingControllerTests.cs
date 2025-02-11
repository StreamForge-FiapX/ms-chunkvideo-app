using Moq;
using Xunit;
using MsChunkVideoApp.Controllers;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MsChunkVideoApp.Tests
{
    public class VideoProcessingControllerTests
    {
        private readonly Mock<IProcessVideoUseCase> _mockProcessVideoUseCase;
        private readonly VideoProcessingController _controller;

        public VideoProcessingControllerTests()
        {
            // Initialize the mock for IProcessVideoUseCase
            _mockProcessVideoUseCase = new Mock<IProcessVideoUseCase>();

            // Initialize the controller with the mocked use case
            _controller = new VideoProcessingController(_mockProcessVideoUseCase.Object);
        }

        [Fact]
        public async Task Get_ShouldReturnOk_WhenProcessIsCalled()
        {
            // Arrange: Set up the mock to return a completed task (simulating a successful process)
            _mockProcessVideoUseCase.Setup(useCase => useCase.Process()).Returns(Task.CompletedTask);

            // Act: Call the GET method
            var result = await _controller.Get();

            // Assert: Verify that the result is Ok()
            var okResult = Assert.IsType<OkResult>(result); // Should be an OkResult (HTTP 200)
        }

        [Fact]
        public async Task PostAsync_ShouldReturnOk_WhenProcessIsCalled()
        {
            // Arrange: Set up the mock to return a completed task (simulating a successful process)
            _mockProcessVideoUseCase.Setup(useCase => useCase.Process()).Returns(Task.CompletedTask);

            // Act: Call the POST method
            var result = await _controller.PostAsync();

            // Assert: Verify that the result is Ok()
            var okResult = Assert.IsType<OkResult>(result); // Should be an OkResult (HTTP 200)
        }

        // Optionally, test the PUT method if needed
        [Fact]
        public void Put_ShouldNotThrowException_WhenCalledWithValidParameters()
        {
            // Arrange: Here, we don't need to mock anything for the PUT method as it's currently empty

            // Act & Assert: Call the PUT method (no processing is needed for testing)
            var exception = Record.Exception(() => _controller.Put(5, "test"));

            // Assert: Verify that no exception is thrown
            Assert.Null(exception);
        }
    }
}
