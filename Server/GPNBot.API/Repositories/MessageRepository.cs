using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;

using Serilog;

using GPNBot.API.Models;
using GPNBot.API.Services;
using GPNBot.API.Tools;
using GPNBot.API.Extensions;
using GPNBot.API.Commands;
using GPNBot.API.WS;
using GPNBot.API.WS.Model;

namespace GPNBot.API.Repositories
{
    public class MessageRepository : Repository<Message>
    {
        protected new readonly string repository = "MessageRepository";
        protected readonly IMemoryCache _cache;
        private const int days = 31;
        private readonly TimeSpan ts = TimeSpan.FromDays(days);

        public MessageRepository(IConnectionService connectionService, IMemoryCache memoryCache) : base(connectionService)
        {
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<(bool, string, long, List<Message>, bool, bool)> GetAsync(
            Guid userGuid, Guid serviceGuid, 
            string queryString, 
            MessageParams messageParams,
            long limit)
        {
            var queryFilters = queryString.QueryFilters();
            var fromIdFilter = queryFilters.FirstOrDefault(i => i.Property.EqIgnoreCase(nameof(Message.ID)));
            var fromIDStr = fromIdFilter?.Value?.ToString() ?? "0";
            var operatorStr = fromIdFilter?.Operator ?? ">";

            var dictWhere = Condition.Create(nameof(Message.UserGuid), userGuid);
            var isStartFromID = long.TryParse(fromIDStr, out var fromID);
            if(isStartFromID)
                dictWhere.AddCondition(nameof(Message.ID), new Condition { Operator = operatorStr, Value = fromID });

            var orderDict = new Dictionary<string, string>() { { nameof(Message.ID) , "desc" } };

            var (success, error, count, results) = await GetAsync(dictWhere, setConditionOr:null, orderDict, limit).ConfigureAwait(false);
            if (!success) return (false, error, 0, null, false, false);

            // make Hello (in Context)
            var isStartFromZero = !isStartFromID;
            if(count == 0 && isStartFromZero)
            { 
                var hello = Hello(messageParams.Hello, userGuid, serviceGuid);
                (success, error, hello) = await PostHelloAsync(hello).ConfigureAwait(false);
                    if (!success) return (false, error, 0, null, false, false);
                return (true, null, 1, new List<Message> { hello }, true, true);
            }

            return (true, null, count, results, false, isStartFromZero);
        }

        public async Task<(bool, string, Message)> GetLastAsync(Guid userGuid)
        { 
            var dictWhere = Condition.Create(nameof(Message.UserGuid), userGuid);
            var orderDict = new Dictionary<string, string>() { { nameof(Message.ID) , "desc" } };
            var (success, error, _, results) = await GetAsync(dictWhere, setConditionOr:null, orderDict, limit:1).ConfigureAwait(false);
            if (!success) return (false, error, null);

            return (true, null, results.FirstOrDefault());
        }

        public override async Task<(bool, string, long, List<Message>)> GetAsync(
            Dictionary<string, Condition[]> dictWhere, 
            CSSet setConditionOr = null, 
            Dictionary<string, string> dictOrder = null, 
            long limit = 0,
            bool onlyCount = false)
        {
            var (success, error, count, results) = await base.GetAsync(dictWhere, setConditionOr, dictOrder, limit, onlyCount:onlyCount).ConfigureAwait(false);
            if (!success) return (false, error, 0, null);

            var messages = new List<Message>();
            foreach (var result in results)
            {
                var message = JsonSerializer.Deserialize<Message>(result.JsonData, GPNJsonSerializer.Option()); 
                PrepareForClient(message, result);
                messages.Add(message);
            }

            return (true, null, count, messages);
        }

        public async Task<(bool, string, Message)> PostHelloAsync(Message message)
        {
            message.JsonData = JsonSerializer.Serialize(message, GPNJsonSerializer.Option());

            var (success, error, newData) = await base.PostAsync(message).ConfigureAwait(false);
            if (!success) return (false, error, null);

            PrepareForClient(message, newData);
            return (true, null, message);
        }
        public async Task<(bool, string, Message)> PostMessageAsync(Message message, MessageParams messageParams)
        {
            if(message.GUID.Equals(Guid.Empty)) return (false, "message.GUID.Equals.Empty", null);
            if(message.Date < DateTime.UtcNow.AddDays(-1)) return (false, "DateTime.Utc.error", null);

            if(message.ServiceGuid is null) message.ServiceGuid = Guid.Empty;
            message.IsSending = false;
            message.JsonData = JsonSerializer.Serialize(message, GPNJsonSerializer.Option());

            var (successLast, errorLast, last) = await GetLastAsync(message.UserGuid).ConfigureAwait(false);
            if (!successLast) return (false, errorLast, null);
            if(last is null)
            {
                var hello = Hello(messageParams.Hello, message.UserGuid, (Guid)message.ServiceGuid);
                hello.JsonData = JsonSerializer.Serialize(hello, GPNJsonSerializer.Option());
                var (successHello, errorHello, _) = await PostAsync(hello).ConfigureAwait(false);
                if (!successHello) return (false, errorHello, null);
                await Task.Delay(1000).ConfigureAwait(false);
            }

            var (success, error, newData) = await base.PostAsync(message).ConfigureAwait(false);
            if (!success) return (false, error, null);

            PrepareForClient(message, newData);
            return (true, null, message);
        }

        public async Task<(bool, string, Message)> PostAnswerAsync(WebSocketHandler webSocketHandler, Message message, string login)
        {
            if(message.GUID.Equals(Guid.Empty)) return (false, "message.GUID.Equals.Empty", null);
            if(message.Date < DateTime.UtcNow.AddDays(-1)) return (false, "DateTime.Utc.error", null);

            if(message.ServiceGuid is null) message.ServiceGuid = Guid.Empty;
            message.IsSending = false;
            message.JsonData = JsonSerializer.Serialize(message, GPNJsonSerializer.Option());


            var (success, error, newData) = await base.PostAsync(message).ConfigureAwait(false);
            if (!success) return (false, error, null);

            PrepareForClient(message, newData);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if(webSocketHandler != null && login != null)
            {
                var x = WSMessage<Message>.EventNew(message);
                webSocketHandler.SendMessageAsync(login, x);
            }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return (true, null, message);
        }


        public static async Task<(bool, string)> UpdateAiScoreAsync(IConnectionService connectionService, Message message, string log)
        {
            if(!message.SourceId.HasValue || message.SourceId == 0)
                return (false, $"{nameof(Message.SourceId)} не найден.");
            
            var aiLog = new AILog() { MessageID = (ulong)message.SourceId, UserMeasuring = log };
            var aiLogRepository = new Repository<AILog>(connectionService);
            var (success, error, _) = await aiLogRepository.PostAsync(aiLog).ConfigureAwait(false);
            if (!success) return (false, error);

            return (true, null);
        }

        private void PrepareForClient(Message message, Message source)
        { 
            message.ID = source.ID;
            message.Created = DateTime.SpecifyKind(source.Created ?? DateTime.MinValue, DateTimeKind.Utc);
            message.JsonData = null;
            if(message.ServiceGuid.Equals(Guid.Empty)) message.ServiceGuid = null;
        }

        public static Message Hello(string body, Guid userGuid, Guid serviceGuid, string commandText = null)
        { 
            Guid serviceGuidContext = Guid.Empty;
            if(ServiceItemRepository.GetServiceItems().Any(s => s.GUID == serviceGuid))
                serviceGuidContext = serviceGuid;

            return new Message
            {
                UserGuid = userGuid,
                ServiceGuid = serviceGuidContext,
                Date = DateTime.UtcNow,
                GUID = Guid.NewGuid(),
                Body = body,
                Command = 0,
                Commands = new List<CommandItem>(),
                CommandText = commandText,
                IsFile = false,
                IsSending = false,
                IsMy = false,
            };
        }

        public static ServiceItem GetServiceItem(Message source)
        { 
            return ServiceItemRepository
                    .GetServiceItems()
                    .FirstOrDefault(s => s.GUID == (source.ServiceGuid ?? Guid.Empty));
        }
    }
}
