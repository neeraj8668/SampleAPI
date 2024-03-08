namespace Sample.API.Services
{
    public class UserService : IReadonlyUserSevice 
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly SampleDatabaseSettings _dbSettings;
        private readonly IMongoDatabase _mongoDatabase;

        public UserService(IOptions<SampleDatabaseSettings> dbSettings, IMongoClient mongoClient)
        {
            _dbSettings = dbSettings.Value;
            _mongoDatabase = mongoClient.GetDatabase(_dbSettings.DatabaseName);
            _userCollection = _mongoDatabase.GetCollection<User>(_dbSettings.UserCollectionName);
        }

        public async Task<List<User>> GetAllAsync() =>
            await _userCollection.Find(_ => true).ToListAsync();


        public async Task<User?> GetAsync(string id)
        {
            return await _userCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }
    }

    public class UserUpdateService : IUserModifySevice
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly SampleDatabaseSettings _dbSettings;
        private readonly IMongoDatabase _mongoDatabase;

        public UserUpdateService(IOptions<SampleDatabaseSettings> dbSettings, IMongoClient mongoClient)
        {
            _dbSettings = dbSettings.Value;
            _mongoDatabase = mongoClient.GetDatabase(_dbSettings.DatabaseName);
            _userCollection = _mongoDatabase.GetCollection<User>(_dbSettings.UserCollectionName);
        }

        public async Task CreateAsync(User newUser) =>
            await _userCollection.InsertOneAsync(newUser);

        public async Task UpdateAsync(string id, User updatedUser) =>
            await _userCollection.ReplaceOneAsync(x => x.Id == id, updatedUser);

        public async Task RemoveAsync(string id) =>
            await _userCollection.DeleteOneAsync(x => x.Id == id);
    }
}
