using Moq;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Data;

namespace Infra.Tests
{
    public class ChunkMetadataAdapterTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IDbConnection> _mockConnection;
        private readonly ChunkMetadataAdapter _chunkMetadataAdapter;

        public ChunkMetadataAdapterTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConnection = new Mock<IDbConnection>();

            // Setup the mock configuration to return a fake connection string
            //_mockConfiguration.Setup(c => c.GetConnectionString("DefaultConnection")).Returns("Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase");
            _mockConfiguration.Setup(config => config.GetConnectionString("DefaultConnection"))
               .Returns("Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase");

            // Initialize the ChunkMetadataAdapter with the mock configuration
            _chunkMetadataAdapter = new ChunkMetadataAdapter(_mockConfiguration.Object);
        }

        [Fact]
        public void SaveChunk_ShouldExecuteInsertStatement()
        {
            // Arrange
            var chunk = new Chunk
            {
                ChunkName = "Chunk1",
                VideoName = "Video1",
                DestinationBucket = "Bucket1"
            };

            // We mock the Execute method of IDbConnection (which Dapper uses)
            _mockConnection.Setup(conn => conn.Execute(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<System.Data.CommandType?>()))
                .Returns(1); // Simulate successful execution by returning a number of affected rows

            // Act
            _chunkMetadataAdapter.SaveChunk(chunk);


            // Assert
            _mockConnection.Verify(conn => conn.Execute(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<System.Data.CommandType?>()), Times.Once);

            // Manually check the SQL query passed to Execute
            _mockConnection.Invocations
                .Where(invocation => invocation.Method.Name == "Execute")
                .ToList()
                .ForEach(invocation =>
                {
                    var sql = invocation.Arguments[0] as string;
                    Assert.Contains("INSERT INTO chunk", sql);
                });
        }
    }
}
