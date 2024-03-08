
namespace Sample.API.Models
{
    /// <summary>
    /// class to support Seeding of Mongo collection data
    /// </summary>
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
            var dbSettings = serviceProvider.GetRequiredService<IOptions<SampleDatabaseSettings>>().Value;
            var mongoDatabase = mongoClient.GetDatabase(dbSettings.DatabaseName);
            var usersCollection = mongoDatabase.GetCollection<User>(dbSettings.UserCollectionName);

             

            if (usersCollection.Find(_ => true).Any())
            {
                return;   // DB has been seeded
            }

            InsertUser(usersCollection, "Amit", "amit@yopmail.com");
        }
         
      
        private static void InsertUser(IMongoCollection<User> usersCollection, string userName, string email )
        {
            var user = new User
            {
                Name = userName,
                Email=email,
                IsActive=true
            };

            usersCollection.InsertOne(user);
        }
    }
}
