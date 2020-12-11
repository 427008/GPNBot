using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using GPNBot.API.Repositories;
using GPNBot.API.WS;
using GPNBot.API.Models;
using GPNBot.API.Commands;
using GPNBot.API.Tools;
using GPNBot.API.Extensions;

namespace GPNBot.API.Services
{
    public class CommnadExecutor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;
        private readonly UsersRepository _users;

        private readonly MessageParams _messageParams;
        private readonly Dialog _dialog;

        public CommnadExecutor(
            IServiceProvider serviceProvider,
            IMemoryCache memoryCache,
            UsersRepository users,
            IOptions<MessageParams> messageOptions,
            IOptions<Dialog> dialogOptions
            )
        { 
            _serviceProvider = serviceProvider;
            _cache = memoryCache;
            _users = users;
            _messageParams = messageOptions.Value;
            _dialog = dialogOptions.Value;
        }

        public async Task RunTaskAsync(long commandCode, Message nextMessage)
        { 
            var (successUser, user) = _users.GetUser(nextMessage.UserGuid);
            if(!successUser) return;

            using var scope = _serviceProvider.CreateScope();
            var connectionService = scope.ServiceProvider.GetRequiredService<IConnectionService>();
            var webSocketHandler = scope.ServiceProvider.GetRequiredService<WebSocketHandler>();

            switch (commandCode)
            {
                case 1:
                case 2:
                    var answerService = scope.ServiceProvider.GetRequiredService<AnswerService>();
                    await RunSetServiceAsync(nextMessage, user, connectionService, webSocketHandler, answerService).ConfigureAwait(false);
                    break;
                case 4:
                    await RunNextAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);
                    break;
                case 5:
                    Log.Information("no mail");
                    break;
                case 6:
                    await RunTemplateAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);
                    break;
                case 7:
                    await RunActionAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);
                    break;
                case 8:
                    await RunRedirectAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);
                    break;
                default:
                    await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);
                    break;
            }
        }

        private async Task<(bool, string, Message)> RunTemplateAsync(Message nextMessage, User user, IConnectionService connectionService, WebSocketHandler webSocketHandler)
        { 
            var template = _dialog.Templates.Where(t => t.CommandText == (nextMessage.CommandText ?? "")).FirstOrDefault();
            if(template is null)
                return await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);

            nextMessage.GUID = Guid.NewGuid();
            nextMessage.CommandText = template.CommandText;
            nextMessage.Commands = template.Commands;
            nextMessage.Body = template.Body;

            var messageRepository = new MessageRepository(connectionService, _cache);
            return await messageRepository.PostAnswerAsync(webSocketHandler, nextMessage, user.Login).ConfigureAwait(false);
        }

        private async Task<(bool, string, Message)> RunNextAsync(Message nextMessage, User user, IConnectionService connectionService, WebSocketHandler webSocketHandler)
        {
            var service = ServiceItemRepository.GetServiceItems().
                FirstOrDefault(s => s.GUID.Equals(nextMessage.ServiceGuid));

            var (hasQueue, userMessages) = _users.GetMessages(nextMessage.UserGuid, QueueType.information);
            if(!hasQueue || userMessages.Count == 0)
            { // end of queue
                if(service?.Id == 1)
                {
                    nextMessage.Command = 6;
                    nextMessage.CommandText = "220099";
                    return await RunTemplateAsync(nextMessage, user, connectionService, webSocketHandler);
                }

                // for service != PI no template ..
                return await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);
            }

            var answers = new List<Message>();
            for(int i = 0; i < 3; i++)
            { 
                if (userMessages.Count == 0) break;
                answers.Add(userMessages.Dequeue());
            }

            if(userMessages.Count != 0)
                answers.Last().Commands = CommandRepository.GetNext();
            if(service?.Id == 1)
                answers.Last().Commands.Add(new CommandItem(6) { CommandText = "200", Title = "В начало", Template = "Перехожу в начало" });

            var now = DateTime.Now;                
            foreach(var answer in answers)
            {
                var tiks = now.Ticks - answer.Created.Value.Ticks;
                answer.Date = answer.Date.AddTicks(tiks);
                    
                var messageRepository = new MessageRepository(connectionService, _cache);
                var (success, error, _) = await messageRepository.PostAnswerAsync(webSocketHandler, answer, user.Login).ConfigureAwait(false);
                if(!success) return (false, error, null);
                // log error
            }

            _users.SetLastMessageID(nextMessage.UserGuid);
            return (true, null, null);
        }

        private async Task<(bool, string, Message)> RunErrorAsync(Message nextMessage, User user, IConnectionService connectionService, WebSocketHandler webSocketHandler)
        {
            nextMessage.Body = _messageParams.CommandError;
            nextMessage.Commands = null;
            var messageRepository = new MessageRepository(connectionService, _cache);
            return await messageRepository.PostAnswerAsync(webSocketHandler, nextMessage, user.Login).ConfigureAwait(false);
        }
    
        private async Task<(bool, string, Message)> RunRedirectAsync(Message nextMessage, User user, IConnectionService connectionService, WebSocketHandler webSocketHandler)
        {
            var nextCommandText = "220010";

            Message commandTemplate = _dialog.Templates.Where(t => t.CommandText == nextCommandText).FirstOrDefault();
            if (commandTemplate == null)
                return await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);

            var template = new Message
            {
                GUID = Guid.NewGuid(),
                UserGuid = nextMessage.UserGuid,
                ServiceGuid = nextMessage.ServiceGuid,
                Date = nextMessage.Date,
                Command = nextMessage.Command,
                Iid = nextMessage.Iid,
                Body = commandTemplate.Body,

                CommandText = commandTemplate?.CommandText,
                Commands = commandTemplate?.Commands,
            };

            var messageRepository = new MessageRepository(connectionService, _cache);
            return await messageRepository.PostAnswerAsync(webSocketHandler, template, user.Login).ConfigureAwait(false);
        }

        public async Task<(bool, string, Message)> RunHelloTemplateAsync(Message nextMessage, User user, ServiceItem service, 
            IConnectionService connectionService, WebSocketHandler webSocketHandler)
        { 
            if(service.Id == 1) 
                nextMessage.CommandText = "200";
            else if(service.Id == 2) 
                nextMessage.CommandText = "100";
            else if(service.Id == 3)
                nextMessage.CommandText = "300";
            else if(service.Id == 4)
                nextMessage.CommandText = "400";
            else
                nextMessage.CommandText = "999";

            return await RunTemplateAsync(nextMessage, user, connectionService, webSocketHandler);
        }

        private async Task<(bool, string, Message)> RunSetServiceAsync(Message nextMessage, User user, 
            IConnectionService connectionService, WebSocketHandler webSocketHandler, AnswerService _answerService)
        {
            if(!Guid.TryParse(nextMessage.CommandText ?? "", out var serviceGuid))
                return await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);

            var service = ServiceItemRepository.GetServiceItems().
                FirstOrDefault(s => s.GUID.Equals(serviceGuid) || s.GUID.Equals(nextMessage.ServiceGuid));

            if(service != null && service.Id != 1)
            { // valid ServiceGuid (for service != PI) - say hello

                _users.ClearMessages(nextMessage.UserGuid, QueueType.category); // TODO if(!) {LOG}
                nextMessage.ServiceGuid = service.GUID; 
                return await RunHelloTemplateAsync(nextMessage, user, service, connectionService, webSocketHandler);
            }

            bool hasQueue;
            Queue<Message> userMessages;
            var messageRepository = new MessageRepository(connectionService, _cache);
            if(service is null)
            {
                LogAIResult(connectionService, nextMessage, "wrong serviceID");

                (hasQueue, userMessages) = _users.GetMessages(nextMessage.UserGuid, QueueType.service);
                if (!hasQueue || userMessages.Count == 0)
                {
                    LogAIResult(connectionService, nextMessage, "everything was wrong with serviceID");

                    nextMessage.Body = _messageParams.NotFound;
                    nextMessage.CommandText = null;
                }
                else
                    nextMessage = userMessages.Dequeue();

                return await messageRepository.PostAnswerAsync(webSocketHandler, nextMessage, user.Login).ConfigureAwait(false);
            }

            if(!nextMessage.ServiceGuid.HasValue) 
                LogAIResult(connectionService, nextMessage, $"let set serviceID={service.Id}");
            
            // valid ServiceGuid (service == PI)
            nextMessage.ServiceGuid = service.GUID; 
            
            _users.ClearMessages(nextMessage.UserGuid, QueueType.service); // TODO if(!) {LOG}
            _users.ClearMessages(nextMessage.UserGuid, QueueType.category); // TODO if(!) {LOG}

            //(hasQueue, userMessages) = _users.GetMessages(nextMessage.UserGuid, QueueType.category);
            //if (!hasQueue || userMessages.Count == 0)
            //{
            //    LogAIResult(connectionService, nextMessage, "everything was wrong with QuestionCategory");

            //    nextMessage.Body = _messageParams.NotFound;
            //    nextMessage.CommandText = null;
            //}
            //else
            //{
            //    nextMessage = userMessages.Dequeue();
            //    _users.ClearMessages(nextMessage.UserGuid, QueueType.category); // TODO if(!) {LOG}
            //}

            if(!nextMessage.SourceId.HasValue)
            { // new empty context - say hello
                nextMessage.Command = 6;
                nextMessage.CommandText = "200";
                return await RunTemplateAsync(nextMessage, user, connectionService, webSocketHandler);
            }
            
            //else if(nextMessage.QuestionCategory == 2) 
            // let ask AI question = body of old message (id=SourceId)
            var (isMessageFound, errorGettingMessage, messageWithQuestion) = await messageRepository.GetAsync((ulong)nextMessage.SourceId).ConfigureAwait(false);
            if(!isMessageFound)
            { // wrong source..
                nextMessage.Command = 6;
                nextMessage.CommandText = "220099";
                return await RunTemplateAsync(nextMessage, user, connectionService, webSocketHandler);
            }

            messageWithQuestion = JsonSerializer.Deserialize<Message>(messageWithQuestion.JsonData, GPNJsonSerializer.Option());
            messageWithQuestion.ServiceGuid = service.GUID;

            var (successParce, queryType, iid) = PresetQuestionFilter.ParseQuestion(messageWithQuestion.Body);
            if(successParce && queryType == 5)
                nextMessage.QuestionCategory = 3;
            else
            { 
                if(queryType > 0 && queryType < 5)
                    nextMessage.QuestionCategory = 2;
                else
                    nextMessage.QuestionCategory = 1;
            }


            if(nextMessage.QuestionCategory == 1)
            {
                nextMessage.Body = "поиск..";
                nextMessage.CommandText = null;
                var (success, error, _) = await messageRepository.PostAnswerAsync(webSocketHandler, nextMessage, user.Login).ConfigureAwait(false);

                success = await _answerService.GetSearch(messageWithQuestion, user, service.Id).ConfigureAwait(false);
                if(success) await RunNextAsync(nextMessage, user, connectionService, webSocketHandler);
                return (success, error, null);
            }
            else if(nextMessage.QuestionCategory == 2)
            {
                await _answerService.GetAggregate(messageWithQuestion, user, service.Id);
                return (true, null, null);
            }
            else // nextMessage.QuestionCategory == 3
            {
                nextMessage.Command = 6;
                nextMessage.CommandText = "220010";
                nextMessage.Iid = iid;

                return await RunTemplateAsync(nextMessage, user, connectionService, webSocketHandler);
            }

            static void LogAIResult(IConnectionService connectionService, Message nextMessage, string userMeasuring)
            {
                var task = new Task(async () =>
                {
                    await MessageRepository.UpdateAiScoreAsync(connectionService, nextMessage, userMeasuring).ConfigureAwait(false);
                });
                task.Start();
            }
        }

        private async Task<(bool, string, Message)> RunActionAsync(Message nextMessage, User user, IConnectionService connectionService, WebSocketHandler webSocketHandler)
        {
            var commandText = nextMessage.CommandText ?? "";
            var commandParts = commandText.Split(';');

            Message commandTemplate = null;
            bool success = commandParts.Length >= 2;
            if(success)
            {
                var nextCommandText = commandParts[0];
                commandTemplate = _dialog.Templates.Where(t => t.CommandText == nextCommandText).FirstOrDefault();
            }
            if(!success || commandTemplate == null)
                return await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);

            string body = null, error = null;
            var action = commandParts[1];
            if (action.EqIgnoreCase("initiativeList"))
            { 
                var email = _users.GetEmail(nextMessage.UserGuid);
                (success, error, body) = await PI.GetInitiativeListAsync(_cache, email).ConfigureAwait(false);
                if(!success)
                { 
                    Log.Error(error);
                }
            }
            else if (action.EqIgnoreCase("activity"))
            { 
                (success, error, body) = await PI.GetInitiativeActivityAsync(_cache, nextMessage.Iid).ConfigureAwait(false);
            }
            else if (action.EqIgnoreCase("description"))
            { 
                (success, error, body) = await PI.GetInitiativeDescriptionAsync(_cache, nextMessage.Iid).ConfigureAwait(false);
            }
            else if (action.EqIgnoreCase("status"))
            { 
                (success, error, body) = await PI.GetInitiativeStatusAsync(_cache, nextMessage.Iid).ConfigureAwait(false);
            }
            else if (action.Equals("email", StringComparison.InvariantCultureIgnoreCase))
            { 
                success = false;
            }

            if(!success)
                return await RunErrorAsync(nextMessage, user, connectionService, webSocketHandler).ConfigureAwait(false);

            var template = new Message
            {
                GUID = Guid.NewGuid(),
                UserGuid = nextMessage.UserGuid,
                ServiceGuid = nextMessage.ServiceGuid,
                Date = nextMessage.Date,
                Command = nextMessage.Command,
                Iid = nextMessage.Iid,

                CommandText = commandTemplate?.CommandText,
                Commands = commandTemplate?.Commands,
                Body = body
            };

            var messageRepository = new MessageRepository(connectionService, _cache);
            return await messageRepository.PostAnswerAsync(webSocketHandler, template, user.Login).ConfigureAwait(false);
        }
    }
}
