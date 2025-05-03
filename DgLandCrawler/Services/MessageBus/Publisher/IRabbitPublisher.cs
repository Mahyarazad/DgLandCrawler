namespace DgLandCrawler.Services.MessageBus.Publisher
{
    public interface IRabbitPublisher
    {
        Task PublishAsync<T>(T message, string routingKey, string exchange = "");
    }
}
