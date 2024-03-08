using Microsoft.Extensions.Options;

namespace Sample.API.Options
{
    public class SampleDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string UserCollectionName { get; set; } = null!;
        public string ProductCollectionName { get; set; } = null!;

    }
}
