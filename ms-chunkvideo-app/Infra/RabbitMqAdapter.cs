using Domain.Gateway;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Infra
{
    public class RabbitMqAdapter : IMessageQueuePort
    {
        private string _hostName;
        private string _user;
        private string _password;
        private string _queueNameIn;
        private string _queueNameOut;

        public RabbitMqAdapter(IConfiguration configuration)
        {
            _hostName = configuration["RabbitMQ:HostName"];
            _user = configuration["RabbitMQ:User"];
            _password = configuration["RabbitMQ:Password"];
            _queueNameIn = configuration["RabbitMQ:QueueNameIn"];
            _queueNameOut = configuration["RabbitMQ:QueueNameOut"];

        }

        public string ConsumeMessage()
        {
            string message = "";

            ConnectionFactory factory = new();


            factory.Uri = new Uri($"amqp://{_user}:{_password}@{_hostName}");
            factory.ClientProvidedName = "MsChunkVideoApp";

            IConnection connection = factory.CreateConnection();

            IModel channel = connection.CreateModel();

            string exchangeName = "VideoExchange";
            string routingkey = "video-routing-key";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(_queueNameIn, false, false, false, null);
            channel.QueueBind(_queueNameIn, exchangeName, routingkey, null);
            channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, args) =>
            {
                var body = args.Body.ToArray();

                message = Encoding.UTF8.GetString(body);

                channel.BasicAck(args.DeliveryTag, false);
            };

            string consumerTag = channel.BasicConsume(_queueNameIn, false, consumer);

            channel.BasicCancel(consumerTag);

            channel.Close();
            connection.Close();

            return message;
        }

        public void PublishMessage(string message)
        {
            ConnectionFactory factory = new();


            factory.Uri = new Uri($"amqp://{_user}:{_password}@{_hostName}");
            factory.ClientProvidedName = "MsChunkVideoApp";

            IConnection connection = factory.CreateConnection();

            IModel channel = connection.CreateModel();

            string exchangeName = "ChunkExchange";
            string routingkey = "chunk-routing-key";

            channel.ExchangeDeclare(exchangeName, ExchangeType.Direct);
            channel.QueueDeclare(_queueNameOut, false, false, false, null);
            channel.QueueBind(_queueNameOut, exchangeName, routingkey, null);            

            byte[] messageBodyBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchangeName, routingkey, null, messageBodyBytes);

            channel.Close();
            connection.Close();
        }
    }
}
