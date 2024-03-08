 

namespace Sample.Shared.Options
{
    public class RabbitMQSettings
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Port { get; set; } = null!;
        public string VirtualHost { get; set; } = null!;
        public string QueueName { get; set; } = null!;
        public string ExchangeName { get; set; } = null!;
    }
}
