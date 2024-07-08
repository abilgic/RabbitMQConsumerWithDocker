using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
namespace RabbitMQConsumer
{
    public class RabbitMqConsumerService : BackgroundService
    {
        private readonly ILogger<RabbitMqConsumerService> _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly BlockingCollection<string> _messageQueue;


        public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger)
        {
            _logger = logger;
            _messageQueue = new BlockingCollection<string>();
            InitializeRabbitMqListener();
        }


        private void InitializeRabbitMqListener()
        {
            var factory = new ConnectionFactory() { HostName = "host.docker.internal" }; // Adjust the hostname if necessary
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();


            _channel.QueueDeclare(queue: "myQueue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);


            _logger.LogInformation("RabbitMQ connection and channel initialized.");
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();


            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                _messageQueue.Add(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };


            _channel.BasicConsume("myQueue", false, consumer);


            return Task.CompletedTask;
        }


        public string GetMessage()
        {
            _messageQueue.TryTake(out var message, TimeSpan.FromSeconds(1));
            return message;
        }


        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}


