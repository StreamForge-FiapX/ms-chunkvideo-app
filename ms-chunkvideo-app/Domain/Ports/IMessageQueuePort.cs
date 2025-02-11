using Domain.Entities;

namespace Domain.Gateway
{
    public interface IMessageQueuePort
    {
        string ConsumeMessage();
        void PublishMessage(string message);
    }
}
