using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using GPNBot.API.Repositories;
using GPNBot.API.WS;
using GPNBot.API.Models;


namespace GPNBot.API.Services
{
    public class AnswerService
    {
        private readonly IConnectionService _connectionService;
        private readonly IMemoryCache _cache;
        private readonly UsersRepository _users;
        private readonly WebSocketHandler _webSocketHandler;
        private readonly MessageParams _messageParams;
        private readonly DBPIParams _dbPIParams;
        private readonly Dialog _dialog;

        public AnswerService(IConnectionService connectionService,
            IMemoryCache memoryCache,
            UsersRepository users,
            WebSocketHandler webSocketHandler,
            IOptions<MessageParams> messageOptions,
            IOptions<Dialog> dialogOptions,
            IOptions<DBPIParams> dbPIParamsOptions
            )
        { 
            _connectionService = connectionService;
            _cache = memoryCache;
            _users = users;
            _webSocketHandler = webSocketHandler;
            _messageParams = messageOptions.Value;
            _dialog = dialogOptions.Value;
            _dbPIParams = dbPIParamsOptions.Value;
        }

        public async Task GetService(Message message, User user)
        { 
            var (success, error, answer) =await Apsolutions.FindServiceByQuestion(message, user, _users, _messageParams).ConfigureAwait(false);
            if (success)
            {
                var messageRepository = new MessageRepository(_connectionService, _cache);
                await messageRepository.PostAnswerAsync(_webSocketHandler, answer, user.Login).ConfigureAwait(false);
            }
            //if(string.IsNullOrEmpty(error)) LOG error
        }

        public async Task GetAggregate(Message message, User user, long serviceId)
        { 
            var (success, error, answer) = await Apsolutions.FindAggregateByQuestion(message, serviceId, user, _users, _messageParams).ConfigureAwait(false);
            if(success)
            {
                var messageRepository = new MessageRepository(_connectionService, _cache);
                await messageRepository.PostAnswerAsync(_webSocketHandler, answer, user.Login).ConfigureAwait(false);
            }
            //if(string.IsNullOrEmpty(error)) LOG error
        }

        public async Task<bool> GetSearch(Message message, User user, long serviceId)
        { 
            bool success = false;
            string error;
            Message answer;
            (success, error, answer) = await Apsolutions.FindReferenceDataByQuestion(message, serviceId, user, _users, _messageParams, _dbPIParams).ConfigureAwait(false);
            if(!success && answer != null)
            { 
                var messageRepository = new MessageRepository(_connectionService, _cache);
                await messageRepository.PostAnswerAsync(_webSocketHandler, answer, user.Login).ConfigureAwait(false);
                //if(string.IsNullOrEmpty(error)) LOG error
            }
            return success;
        }
    }
}
