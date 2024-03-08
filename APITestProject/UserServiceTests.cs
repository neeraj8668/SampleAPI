using Microsoft.Extensions.Options;
using Moq;
using MongoDB.Driver;


namespace APITestProject
{
    public class UserServiceTests
    {
        private Mock<IMongoCollection<User>> mockCollection;
        private Mock<IMongoDatabase> mockDatabase;
        private Mock<IMongoClient> mockClient;
        private SampleDatabaseSettings mockDbSettings;
        private UserService userService;
        public UserServiceTests()
        {

            mockCollection = new Mock<IMongoCollection<User>>();
            mockDatabase = new Mock<IMongoDatabase>();
            mockClient = new Mock<IMongoClient>();
            var mockDbSettings = new SampleDatabaseSettings { DatabaseName = "SampleAPI", UserCollectionName = "Users" };
            var mockOptions = Options.Create(mockDbSettings);
            userService = new UserService(mockOptions, mockClient.Object);
        }

        [Fact]
        public async Task GetAsync_ReturnsUser_WithValidId()
        {
            // Arrange
            var validUserId = "65e8320f8461800c3c046a31";
            var expectedUser = new User { Id = validUserId, Name = "Amit", IsActive = true };

            mockCollection.Setup(x => x.Find(It.IsAny<FilterDefinition<User>>(), null))
                          .Returns((IFindFluent<User, User>)new Mock<IAsyncCursor<User>>().Object);
             
             
            mockDatabase.Setup(x => x.GetCollection<User>(It.IsAny<string>(), null)).Returns(mockCollection.Object);
            mockClient.Setup(x => x.GetDatabase(It.IsAny<string>(), null)).Returns(mockDatabase.Object);

            // Act
            var result = await userService.GetAsync(validUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUser, result);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WithInvalidId()
        {
            // Arrange
            var invalidUserId = "invalid-id";

           // mockCollection.Setup(x => x.FindOneAsync(It.IsAny<FilterDefinition<User>>(), null))
                        //  .ReturnsAsync((User)null);

            mockDatabase.Setup(x => x.GetCollection<User>(It.IsAny<string>(), null)).Returns(mockCollection.Object);
            mockClient.Setup(x => x.GetDatabase(It.IsAny<string>(), null)).Returns(mockDatabase.Object);
            // Act
            var result = await userService.GetAsync(invalidUserId);

            // Assert
            Assert.Null(result);
        }
    }
}