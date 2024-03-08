

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sample.Shared.Options
{
    public class RabbitMQSettingsOptions : IConfigureOptions<RabbitMQSettings>
    {
        public const string SectionName = "RabbitMqSetting";

        private readonly IConfiguration _configuration;

        public RabbitMQSettingsOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void Configure(RabbitMQSettings options)
        {
            _configuration
           .GetSection(SectionName)
           .Bind(options);
        }
    }
}
