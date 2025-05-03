using RabbitMQ.Client;

namespace DgLandCrawler.Services.MessageBus.Publisher
{
    public class RabbitPublisher : IRabbitPublisher
    {

        private readonly IChannel _channel;
        public RabbitPublisher(IChannel channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public async Task PublishAsync<T>(T message, string routingKey, string exchange = "")
        {
            try
            {
                // Ensure the queue exists (create it if not already there)
                await _channel.QueueDeclareAsync(
                    queue: routingKey,
                    durable: true, // save to disk so the queue isn’t lost on broker restart
                    exclusive: false, // can be used by other connections
                    autoDelete: false, // don’t delete when the last consumer disconnects
                    arguments: null);

                var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);

                await _channel.BasicPublishAsync(
                        exchange: string.Empty, // default exchange
                        routingKey: routingKey,
                        mandatory: true, // fail if the message can’t be routed
                        basicProperties: new BasicProperties { Persistent = true }, // message will be saved to disk
                        body: bytes);

                Serilog.Log.Information("Sent >> PublishAsync >> {Message}", new { Message = System.Text.Json.JsonSerializer.Serialize(message) });


            }
            catch(Exception ex)
            {
                Serilog.Log.Error("Exception >> PublishAsync >> {Message}", new { Message = ex.Message });
            }
        }
    }
}
