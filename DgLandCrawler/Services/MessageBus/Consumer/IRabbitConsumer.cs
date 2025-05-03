
namespace DgLandCrawler.Services.MessageBus.Consumer
{
    public interface IRabbitConsumer
    {
        Task ConsumeAsync<T>(string queue);
    }
}
