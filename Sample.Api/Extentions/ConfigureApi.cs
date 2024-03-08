
using System.Reflection;

namespace Sample.API.Extentions
{
    public static class ConfigureApi
    {
        /// <summary>
        /// method to configure Services used in the application
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        /// <param name="_logger"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureAPI(this IServiceCollection services,
            IConfiguration configuration, IWebHostEnvironment env, Serilog.ILogger _logger)
        {
            // configuring Database setting options 
            services.ConfigureOptions<SampleDatabaseSettingsOptions>();
            // configuring Database setting options 
            services.ConfigureOptions<RabbitMQSettingsOptions>();

            // configuring CORS options
            services.ConfigureOptions<CQRSSettingsOptions> ();

            var corsOptions = new CQRSOption();
            configuration.GetSection(CQRSSettingsOptions.SectionName).Bind(corsOptions);

            //setting up orgins for cofigured client urls 
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(corsOptions.AllowedHosts)
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });

            // adding Mongoclient service 
            services.AddSingleton<IMongoClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<SampleDatabaseSettings>>().Value;
                var mongoClientSettings = MongoClientSettings.FromUrl(new MongoUrl(settings.ConnectionString));

                // Add authentication credentials
                mongoClientSettings.Credential = MongoCredential.CreateCredential(
                    settings.DatabaseName,
                    settings.Username,
                    settings.Password
                );
               // return new MongoClient(mongoClientSettings);
                return new MongoClient(settings.ConnectionString);
            });
            
            //adding Users and product services
            services.AddSingleton<IReadonlyUserSevice, UserService>();

            services.AddSingleton<ILoggerWrapper, LoggerWrapper>(); // Use the appropriate implementation for ILoggerWrapper


            //adding UPdate service used by consumer
            services.AddSingleton<IUserModifySevice, UserUpdateService>();
            
            //adding User service used by api
            services.AddSingleton<IRabbitMqService, RabbitMQService>();

            //registering validators
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddHostedService<UserConsumerService>();

            //adding controllers
            services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.Filters.Add<ApiExceptionFilterAttribute>(); 
                options.Filters.Add<ValidateModelAttribute>();  
            }).AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null); 
           

            services.AddEndpointsApiExplorer();

            // adding swagger support 
            services.AddSwaggerGen();

            return services;
        }
    }
}
