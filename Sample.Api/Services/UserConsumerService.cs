using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

namespace Sample.API.Services
{
    public class UserConsumerService : BackgroundService
    {
        private readonly IUserModifySevice _userService;
        private readonly RabbitMQSettings _settings;
        private IModel _channel;
        private readonly ILoggerWrapper _logger;

        public UserConsumerService(IOptions<RabbitMQSettings> rabbitMqSettings, IUserModifySevice userService, ILoggerWrapper logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _settings = rabbitMqSettings?.Value ?? throw new ArgumentNullException(nameof(rabbitMqSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UserConsumerService is executing.");

            InitializeRabbitMQConnection();
            return Task.CompletedTask;
        }

        private void InitializeRabbitMQConnection()
        {
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

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += HandleMessage;
            _channel.BasicConsume(queue: _settings.QueueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("Connected to RabbitMQ and started consuming messages.");

        }

        public async void HandleMessage(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                await ProcessMessage(message);
                _channel.BasicAck(ea.DeliveryTag, false); // Acknowledge the message
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {Message}", message);

                _channel.BasicNack(ea.DeliveryTag, false, true); // Reject and requeue the message
            }
        }

        private async Task ProcessMessage(string message)
        {
            // Perform actions based on the stored queue name (_settings.QueueName)
            var evnt = JsonConvert.DeserializeObject<DomainEvent<User>>(message);
            if (evnt != null && !string.IsNullOrEmpty(evnt.EventName))
            {
                if (evnt.EventName == AppConstants.Domain_Event_Create && evnt.Data != null)
                    await _userService.CreateAsync(evnt.Data);
                if (evnt.EventName == AppConstants.Domain_Event_Update && evnt.Data != null)
                {
                    var user = evnt.Data;
                    await _userService.UpdateAsync(user.Id, evnt.Data);
                }
                if (evnt.EventName == AppConstants.Domain_Event_Remove && evnt.Data != null)
                    await _userService.RemoveAsync(evnt.Data.Id);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("UserConsumerService is stopping.");

            await base.StopAsync(cancellationToken);
            _channel.Close();
            _logger.LogInformation("UserConsumerService stopped.");
        }
    }
}
