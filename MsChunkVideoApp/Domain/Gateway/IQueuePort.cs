using Domain.Entities;

namespace Application.UseCases
{
    public interface IQueuePort
    {
        string ConsumeMessage();
        void PublishMessage(string message);
    }
}
