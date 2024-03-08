using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace APITestProject
{
    public class UsersControllerTests
    {
        private Mock<IReadonlyUserSevice> _mockUserService;
        private Mock<IRabbitMqService> rabbitmqService;
        private Mock<ILoggerWrapper> logWrapper;

        private UsersController _controller;
        
        public UsersControllerTests()
        {
            Setup();
        }

        public void Setup()
        {
            _mockUserService = new Mock<IReadonlyUserSevice>();
            rabbitmqService = new Mock<IRabbitMqService>();
            logWrapper = new Mock<ILoggerWrapper>();
            var mockRabbitMQSettings = new RabbitMQSettings
            {
                Host = "localhost",
                Port = "5672",
                Username = "guest",
                Password = "guest"
            };
            var mockOptions = Options.Create(mockRabbitMQSettings);

            _controller = new UsersController(_mockUserService.Object, rabbitmqService.Object, mockOptions,logWrapper.Object);
        }
        [Fact]
        public async Task GetUsers_ReturnsOk_WithUsers()
        {
            // Arrange


            var expectedUsers = new List<User>
            {
                new User { Id = "65e8320f8461800c3c046a31", Name = "Amit" , Email="amit@yopmail.com" , IsActive=true},
                new User { Id = "65e855396b0b2ff3b6d4eb51", Name = "name968", Email="name968@yopmail.com" , IsActive=true }
            };
            _mockUserService.Setup(service => service.GetAllAsync()).ReturnsAsync(expectedUsers);
            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var users = Assert.IsType<List<User>>(okResult.Value);
            Assert.Equal(expectedUsers.Count, users.Count);
        }

        [Fact]
        public async Task GetUser_ReturnsOk_WithValidId()
        {
            // Arrange
            var userId = "65e8320f8461800c3c046a31";
            var expectedUser = new User { Id = userId, Name = "Amit"  };

            _mockUserService.Setup(service => service.GetAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsType<User>(okResult.Value);
            Assert.Equal(expectedUser, user);
        }
        [Fact]
        public async Task GetUser_ReturnsNotFound_WithInvalidId()
        {
            // Arrange
            var userId = "65e855396b0b2ff3b6d4eb512";

            _mockUserService.Setup(service => service.GetAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
        [Fact]
        public async Task PutUser_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            User userInput = null;

            // Act
            var result = await _controller.User("1", userInput);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task PutUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userInput = new User { Id = "1", Name = "Updated User", Email = "updated@example.com" };

            _mockUserService.Setup(x => x.GetAsync(userInput.Id)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.User(userInput.Id, userInput);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task PutUser_ValidInput_ReturnsOkResult()
        {
            // Arrange
            var userInput = new User { Id = "1", Name = "Updated User", Email = "updated@example.com" };
            var existingUser = new User { Id = "1", Name = "Existing User", Email = "existing@example.com" };

            _mockUserService.Setup(x => x.GetAsync(userInput.Id)).ReturnsAsync(existingUser);

            // Act
            var result = await _controller.User(userInput.Id, userInput);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var user = Assert.IsAssignableFrom<User>(okResult.Value);
            Assert.Equal(existingUser, user);
        }
    }
}