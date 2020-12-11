using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using GPNBot.API.Models;
using GPNBot.API.Repositories;
using GPNBot.API.Services;


namespace GPNBot.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServiceController: ControllerBase
    {
        private readonly ILogger<ServiceController> _logger;
        private readonly IConnectionService _connectionService;
        private readonly IMemoryCache _cache;
        private readonly UsersRepository _users;

        public ServiceController(ILogger<ServiceController> logger, 
            IConnectionService connectionService, 
            IMemoryCache memoryCache,
            UsersRepository users)
        {
            _logger = logger;
            _connectionService = connectionService;
            _cache = memoryCache;
            _users = users;
        }

        [HttpGet]
        public JsonResponce Get([FromHeader] string Authorization)
        {
            var (success, _, _, _, _) = _users.ValidateToken(Authorization);
            if(!success) return new JsonResponce { Success = false, Message = "404" };

            var services = ServiceItemRepository.GetServiceItems();
            return new JsonResponce(){ Data = services, Total = services.Count };
        }
    }
}
