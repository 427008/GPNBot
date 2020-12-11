using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using GPNBot.API.Models;
using GPNBot.API.Repositories;
using GPNBot.API.Services;
using GPNBot.API.Commands;
using GPNBot.API.WS;


namespace GPNBot.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController: ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IConnectionService _connectionService;
        private readonly IMemoryCache _cache;
        private readonly UsersRepository _users;
        private readonly AnswerService _answerService;
        private readonly WebSocketHandler _webSocketHandler;
        private readonly MessageParams _messageParams;
        private readonly Dialog _dialog;
        private readonly CommnadExecutor _commnadExecutor;

        public MessageController(
            ILogger<MessageController> logger, 
            IConnectionService connectionService, 
            IMemoryCache memoryCache,
            UsersRepository users,
            WebSocketHandler webSocketHandler,
            CommnadExecutor commnadExecutor,
            AnswerService answerService,
            IOptions<MessageParams> messageOptions,
            IOptions<Dialog> dialogOptions
            )
        {
            _logger = logger;
            _connectionService = connectionService;
            _cache = memoryCache;
            _webSocketHandler = webSocketHandler;
            _users = users;
            _commnadExecutor = commnadExecutor;
            _answerService = answerService;

            _messageParams = messageOptions.Value;
            _dialog = dialogOptions.Value;
        }

        [HttpGet]
        public async Task<JsonResponce> GetAsync([FromQuery] Guid serviceGuid, [FromQuery] long limit, [FromQuery] long start, [FromQuery] long page)
        {
            var login = Request.Headers.TryGetValue("UserLogin", out var userLoginStr) ? userLoginStr.ToString() : "";
            var (success, user) = _users.GetUser(login);
            //_logger.LogInformation("{source}{action}{authorization}{remoteIp}", nameof(MessageController), Request.Method, $"{success}={fio}/{userGuid}", Request.HttpContext.Connection.RemoteIpAddress);
            if(!success) return new JsonResponce { Success = false, Message = "404" };

            if(limit == 0) limit = 300;
            var messageRepository = new MessageRepository(_connectionService, _cache);
            var (successGet, error, count, results, isHello, isStartFromZero) = 
                await messageRepository.GetAsync(user.GUID, serviceGuid, Request.QueryString.Value, _messageParams, limit).ConfigureAwait(false);
            if (!successGet) return new JsonResponce(){ Success = false, Message = error };

            if(isHello)
                return new JsonResponce(){ Data = results, Total = count };

            var (successLast, errorLast, last) = await messageRepository.GetLastAsync(user.GUID).ConfigureAwait(false);
            if (!successLast || last is null) return new JsonResponce { Success = false, Message = _messageParams.CommandError };

            var lastServiceGuid = last.ServiceGuid ?? Guid.Empty;
            var service = ServiceItemRepository.GetServiceItems().FirstOrDefault(s => s.GUID == lastServiceGuid);
            var requiredService = ServiceItemRepository.GetServiceItems().FirstOrDefault(s => s.GUID == serviceGuid);

            if ((service?.Id ?? 0) != (requiredService?.Id ?? 0) && requiredService != null)
            { // Context changed привет из контекста
                var hello = MessageRepository.Hello(_messageParams.Hello, user.GUID, serviceGuid, commandText: requiredService.GUID.ToString());
                (success, error, hello) = await _commnadExecutor.RunHelloTemplateAsync(hello, user, requiredService, _connectionService, _webSocketHandler);
                if (!success) return new JsonResponce(){ Success = false, Message = error };

                results.Add(hello);
                count++;
            }

            results.Reverse();
            return new JsonResponce(){ Data = results, Total = count };
        }

        [HttpPost]
        public async Task<JsonResponce> PostAsync([FromHeader] string Authorization, Message message)
        {
            var login = Request.Headers.TryGetValue("UserLogin", out var userLoginStr) ? userLoginStr.ToString() : "";
            var (success, user) = _users.GetUser(login);
            
            _logger.LogInformation("{source}{action}{authorization}{remoteIp}", nameof(MessageController), Request.Method, $"{success}={user.Fio}/{user.GUID}", Request.HttpContext.Connection.RemoteIpAddress);
            if(!success) return new JsonResponce { Success = false, Message = "404" };
            
            string error = "404";
            message.UserGuid = user.GUID;
            var messageRepository = new MessageRepository(_connectionService, _cache);

            ulong iid;
            long queryType = 0;
            if(message.Command == 0)
            { 
                (success, queryType, iid) = PresetQuestionFilter.ParseQuestion(message.Body);
                if(success && queryType == 5)
                {   
                    message.Command = 6;
                    message.CommandText = "220010";
                    message.Iid = iid;
                }
            }
            if(message.Command != 0)
            { 
                message.IsMy = true;
                message.Commands = null;
                if(message.ValidAnswer.HasValue) message.CommandValid = message.ValidAnswer;

                (success, error, message) = await messageRepository.PostAnswerAsync(_webSocketHandler, message, user.Login).ConfigureAwait(false);
                if (!success) return new JsonResponce(){ Success = false, Message = error };
                
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                var (command, nextMessage) = CommandRepository.GetNextMessage(message);
                _commnadExecutor.RunTaskAsync(command, nextMessage);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return new JsonResponce(){ Data = message };
            }

            message.IsMy = true;
            message.Commands = new List<CommandItem>();
            (success, error, message) = await messageRepository.PostMessageAsync(message, _messageParams).ConfigureAwait(false);
            if (!success) return new JsonResponce(){ Success = false, Message = error };

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var serviceItem = MessageRepository.GetServiceItem(message);
            if (serviceItem is null)
                _answerService.GetService(message, user);
            else if(queryType > 0 && queryType < 5)
                _answerService.GetAggregate(message, user, serviceItem.Id);
            else
            {
                success = await _answerService.GetSearch(message, user, serviceItem.Id).ConfigureAwait(false);
                if(success) _commnadExecutor.RunTaskAsync(4, message);
            }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return new JsonResponce(){ Data = message };
        }
    }
}
