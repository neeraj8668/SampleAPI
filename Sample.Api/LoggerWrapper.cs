using Microsoft.Extensions.Logging;

namespace Sample.API
{
    public interface ILoggerWrapper
    {
        void LogInformation(string message);
        void LogError(Exception ex, string message, string data);
    }

    public class LoggerWrapper : ILoggerWrapper
    {
        private readonly ILogger<UserConsumerService> _logger;

        public LoggerWrapper(ILogger<UserConsumerService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }
        public void LogError(Exception ex,string message, string data)
        {
            _logger.LogError(ex, message, data);
        }
    }
}
