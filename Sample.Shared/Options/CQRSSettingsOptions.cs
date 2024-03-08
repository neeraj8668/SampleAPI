using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Sample.Shared.Options
{
    public class CQRSSettingsOptions : IConfigureOptions<CQRSOption>
    {
        public const string SectionName = "CORS";

        private readonly IConfiguration _configuration;

        public CQRSSettingsOptions(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void Configure(CQRSOption options)
        {
            _configuration
           .GetSection(SectionName)
           .Bind(options);
        }
    }

}
