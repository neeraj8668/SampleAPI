using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

namespace APITestProject
{
    public class UserConsumerServiceTests
    {
        private Mock<ILoggerWrapper> loggerWrapperMock;
        private Mock<IUserModifySevice> userServiceMock;
        
        private UserConsumerService service;
        
        public UserConsumerServiceTests()
        {
            userServiceMock = new Mock<IUserModifySevice>();
            loggerWrapperMock= new Mock<ILoggerWrapper>();

            var rabbitMqSettings = Options.Create(new RabbitMQSettings
            {
                Host = "localhost",
                Port = "5672",
                Username = "guest",
                Password = "guest",
                QueueName = "test_queue"
            });
            service = new UserConsumerService(rabbitMqSettings, userServiceMock.Object, loggerWrapperMock.Object);

        }
        [Fact]
        public async Task ExecuteAsync_StartsService()
        {
          
            
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            await service.StartAsync(cancellationToken);
            await service.StopAsync(cancellationToken);

            // Assert
            loggerWrapperMock.Verify(x => x.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task HandleMessage_ProcessesMessage()
        {
            // Arrange
            var message = JsonConvert.SerializeObject(new DomainEvent<User>
            {
                EventName = AppConstants.Domain_Event_Create,
                Data = new User {   Name = "Amit2", Email= "amit2@yopmail.com", IsActive = true }
            });
            var body = Encoding.UTF8.GetBytes(message);

            var eventArgs = new BasicDeliverEventArgs
            {
                Body = body
            };

           
            var userServiceMock = new Mock<IUserModifySevice>();
            userServiceMock.Setup(x => x.CreateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            service.HandleMessage(null, eventArgs);

            // Assert
            userServiceMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }
    }

}