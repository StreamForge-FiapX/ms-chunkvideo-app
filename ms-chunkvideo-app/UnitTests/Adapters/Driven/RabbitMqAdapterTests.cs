using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Infra.Tests
{
    public class RabbitMqAdapterTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IModel> _mockChannel;
        private readonly Mock<EventingBasicConsumer> _mockConsumer;
        private readonly RabbitMqAdapter _rabbitMqAdapter;

        public RabbitMqAdapterTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConnection = new Mock<IConnection>();
            _mockChannel = new Mock<IModel>();
            _mockConsumer = new Mock<EventingBasicConsumer>(_mockChannel.Object);

            _mockConfiguration.Setup(c => c["RabbitMQ:HostName"]).Returns("localhost");
            _mockConfiguration.Setup(c => c["RabbitMQ:User"]).Returns("user");
            _mockConfiguration.Setup(c => c["RabbitMQ:Password"]).Returns("password");
            _mockConfiguration.Setup(c => c["RabbitMQ:QueueNameIn"]).Returns("queueIn");
            _mockConfiguration.Setup(c => c["RabbitMQ:QueueNameOut"]).Returns("queueOut");

            _rabbitMqAdapter = new RabbitMqAdapter(_mockConfiguration.Object);
        }

        [Fact]
        public void ConsumeMessage_ShouldReturnMessage()
        {
            // Arrange
            string expectedMessage = "Hello from Queue!";
            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(expectedMessage);
            
            // Mocking the RabbitMQ interaction
            _mockChannel.Setup(channel => channel.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()))
                .Returns("consumerTag");

            //_mockConsumer.Setup(consumer => consumer.Received += It.IsAny<EventHandler<BasicDeliverEventArgs>>());

            // Simulating the event being triggered with a mock consumer
            _mockConsumer.Raise(consumer => consumer.Received += null, new object(), new BasicDeliverEventArgs
            {
                Body = messageBodyBytes
            });

            // Act
            var result = _rabbitMqAdapter.ConsumeMessage();

            // Assert
            Assert.Equal(expectedMessage, result);
            _mockChannel.Verify(c => c.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()), Times.Once);
        }

        [Fact]
        public void PublishMessage_ShouldSendMessageToQueue()
        {
            // Arrange
            string messageToPublish = "Test message";

            // Act
            _rabbitMqAdapter.PublishMessage(messageToPublish);

            // Assert
            _mockChannel.Verify(c => c.BasicPublish(
                It.Is<string>(s => s == "ChunkExchange"),  // Exchange Name
                It.Is<string>(s => s == "chunk-routing-key"),  // Routing Key
                It.IsAny<IBasicProperties>(),  // BasicProperties (null here)
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == messageToPublish)),  // Verify the message is sent correctly
                Times.Once);
        }
    }
}
