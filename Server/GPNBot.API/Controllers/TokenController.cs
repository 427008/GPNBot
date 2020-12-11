using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using GPNBot.API.Models;
using GPNBot.API.Repositories;


namespace GPNBot.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokenController: ControllerBase
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IMemoryCache _cache;
        private readonly UsersRepository _users;

        public TokenController(ILogger<TokenController> logger, 
            IMemoryCache memoryCache,
            UsersRepository users)
        {
            _logger = logger;
            _cache = memoryCache;
            _users = users;
        }

        [HttpPost]
        public async Task<JsonResponce> PostAsync(UserLogin login)
        {
            var (success, token, expires) = _users.GetTokenForUser(login.Login, login.Password);
            if(!success) return new JsonResponce { Success = false, Message = "Проверьте логин и пароль.." };

            return await Task.FromResult(new JsonResponce(){ Data = new { token, expires } });
        }

        [Route("/userinfo")]
        [HttpGet]
        public async Task<JsonResponce> GetAsync()
        {
            var login = Request.Headers.TryGetValue("UserLogin", out var userLoginStr) ? userLoginStr.ToString() : "";
            var (success, userGuid, fio, position, employmentDate) = _users.ValidateUser(login);
            if(!success) return new JsonResponce { Success = false, Message = "404" };

            return await Task.FromResult(new JsonResponce(){ Data = new { userGuid, fio, position, employmentDate } });
        }

        [Route("/logout")]
        [HttpPost]
        public async Task<JsonResponce> LogoutAsync()
        {
            var login = Request.Headers.TryGetValue("UserLogin", out var userLoginStr) ? userLoginStr.ToString() : "";
            var (success, userGuid, _, _, _) = _users.ValidateUser(login);
            if(!success) return new JsonResponce { Success = false, Message = "404" };

            return await Task.FromResult(new JsonResponce(){ Data = new { logout = _users.Logout(userGuid) } });
        }

    }
}
