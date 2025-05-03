using RabbitMQ.Client.Events;

namespace DgLandCrawler.Services.MessageBus.Consumer
{
    public interface IConsumerHandler
    {
        Task ConsumeAsync<T>(string queue);
    }
}
