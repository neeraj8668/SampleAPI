using Sample.Shared.Models;

namespace Sample.Shared.Interfaces
{
    public interface IReadonlyUserSevice
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetAsync(string id);
    }
    public interface IUserModifySevice
    {
        Task UpdateAsync(string id, User updatedUser);
        Task RemoveAsync(string id);
        Task CreateAsync(User newUser);
    }
    public interface IRabbitMqService
    {
        Task SendMessage(string message, string queueName);
    }

}
