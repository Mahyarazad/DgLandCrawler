using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DgLandCrawler.Services.MessageBus.Consumer
{
    public class ConsumerHandler : IConsumerHandler
    {   
        private readonly IChannel _channel;
        public ConsumerHandler(IChannel channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        public async Task ConsumeAsync<T>(string queue)
        {
            try
            {
                await _channel.QueueDeclareAsync(
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (sender, args) =>
                {
                    byte[] body = args.Body.ToArray();
                    string message = System.Text.Encoding.UTF8.GetString(body);
                    var messageObject = System.Text.Json.JsonSerializer.Deserialize<T>(message);

                    Serilog.Log.Information("Message Received: {Message}", new { Message = messageObject });

                    await _channel.BasicAckAsync(deliveryTag: args.DeliveryTag,multiple: false);
                };

                await _channel.BasicConsumeAsync(
                    queue: queue,
                    autoAck: false,
                    consumer: consumer
                );
            }
            catch(Exception ex)
            {
                Serilog.Log.Error("Error in ConsumeAsync: {Message}", new { Message = ex.Message });
            }
        }
    }
}
