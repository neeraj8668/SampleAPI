
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;


namespace Sample.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IReadonlyUserSevice _userService;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly IRabbitMqService _rabbitMQService;
        private readonly RabbitMQSettings _settings;
        private readonly ILoggerWrapper _logger;

        public UsersController(IReadonlyUserSevice userService, IRabbitMqService rabbitmqService, IOptions<RabbitMQSettings> rabbitMqSettings,ILoggerWrapper logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _rabbitMQService = rabbitmqService ?? throw new ArgumentNullException(nameof(rabbitmqService));
            _settings = rabbitMqSettings.Value ?? throw new ArgumentNullException(nameof(rabbitMqSettings.Value));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        // GET: api/Users
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<User>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<User>))]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllAsync();
            return users.Any() ? Ok(users) : NotFound();
        }

        /// <summary>
        /// method to get user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET: api/Users/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userService.GetAsync(id);
            return user != null ? Ok(user) : NotFound();
        }

        /// <summary>
        /// method to create a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        // POST: api/Users
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> User([FromBody] User user)
        {
            if (user == null)
                return BadRequest("Invalid request data.");

            var payload = new DomainEvent<User>() { Data = user, EventName = AppConstants.Domain_Event_Create };

            var message = JsonConvert.SerializeObject(payload);

            await _rabbitMQService.SendMessage(message, _settings.QueueName);


            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// method to update a User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userInput"></param>
        /// <returns></returns>
        // PUT: api/Users/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> User(string id, [FromBody] User userInput)
        {
            if (userInput == null)
                return BadRequest("Invalid request data.");

            var user = await _userService.GetAsync(userInput.Id);
            if (user == null)
                return NotFound();

            // Update the user properties
            user.Name = userInput.Name;
            user.Email = userInput.Email;
            user.IsActive = userInput.IsActive;
            var payload = new DomainEvent<User>() { Data = userInput, EventName = AppConstants.Domain_Event_Update };

            var message = JsonConvert.SerializeObject(payload);

            await _rabbitMQService.SendMessage(message, _settings.QueueName);


            return Ok(user);
        }


        /// <summary>
        /// method to delete a user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userService.GetAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            var payload = new DomainEvent<User>() { Data = user, EventName = AppConstants.Domain_Event_Remove };

            var message = JsonConvert.SerializeObject(payload);

            await _rabbitMQService.SendMessage(message, _settings.QueueName);


            return NoContent();
        }

        [HttpDelete("DeactivateUser")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            // Deactivate user logic here
            var existingUser = await _userService.GetAsync(id);
            if (existingUser == null)
                return NotFound();

            existingUser.IsActive = false;

            return Ok(existingUser);
        }

    }
}
