using RabbitMQ.Client;
using System.Text;

namespace Sample.API
{
    public class RabbitMQService : IRabbitMqService
    {
        private readonly IModel _channel;
        private readonly RabbitMQSettings _settings;
        private readonly ILoggerWrapper _logger;

        public RabbitMQService(IOptions<RabbitMQSettings> rabbitMqSettings, ILoggerWrapper logger)
        {
            _settings = rabbitMqSettings?.Value ?? throw new ArgumentNullException(nameof(rabbitMqSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = int.Parse(_settings.Port),
                UserName = _settings.Username,
                Password = _settings.Password
            };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            // Declare the exchange
            _channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: ExchangeType.Direct);

            _channel.QueueDeclare(queue: _settings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Bind the queue to the exchange
            _channel.QueueBind(queue: _settings.QueueName, exchange: _settings.ExchangeName, routingKey: "");
        }

        public Task SendMessage(string message, string queueName)
        {
            return Task.Run(() =>
            {
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: _settings.ExchangeName, routingKey: "", basicProperties: null, body: body);
                _logger.LogInformation($"Message sent to {queueName} via exchange {_settings.ExchangeName}");
            });
        }
    }
}
