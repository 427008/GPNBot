using System;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using Serilog;

using GPNBot.API.Models;
using GPNBot.API.Tools;
using System.Collections.Generic;
using GPNBot.API.Repositories;
using System.Linq;
using GPNBot.API.Commands;

namespace GPNBot.API.Services
{
    public static class Apsolutions
    {
        private static (RestClient, RestRequest) GetRequest(string url)
        {
            var client = new RestClient(url);
            client.Timeout = 4500;

            var request = new RestRequest(Method.POST);
            var TOKEN = "";
            request.AddHeader("Authorization", $"Bearer {TOKEN}");
            request.AddHeader("Content-Type", "application/json");

            return (client, request);
        }

        private static async Task<(bool, string, ApsolutionsCategory)> GetCategorized(Guid Id, string FIO, string Position, DateTime EmploymentDate, string Question)
        {
            var (client, request) = GetRequest("https://semsearch.apsolutions.ru/api/v1/question/classify_extended");

            request.AddJsonBody(new
            {
                Profile = new
                {
                    FIO,
                    Id,
                    Position,
                    EmploymentDate
                },
                QuestionType = "1",
                Question,
                OnlySearchType = false
            });

            try
            {
                var response = await client.ExecuteAsync(request).ConfigureAwait(false);
                var categories = JsonSerializer.Deserialize<ApsolutionsCategory>(response.Content, GPNJsonSerializer.Option());
                return (true, null, categories);
            }
            catch (Exception ex)
            {
                Log.Error("{source}: '{query}'. Error: '{error}'", "Apsolutions", Question, ex.Message);
                return (false, ex.Message, null);
            }
        }

        private static async Task<(bool, string, ApsolutionsAnswer)> SearchText(Guid Id, string FIO, string Position, DateTime EmploymentDate, long ServiceID, string Question)
        {
            var (client, request) = GetRequest("https://semsearch.apsolutions.ru/api/v1/question/search");

            request.AddJsonBody(new
            {
                Profile = new
                {
                    FIO,
                    Id,
                    Position,
                    EmploymentDate
                },
                QuestionType = "text",
                Question,
                ServiceID,
                Count = 10
            });

            try
            {
                var response = await client.ExecuteAsync(request).ConfigureAwait(false);
                var answer = JsonSerializer.Deserialize<ApsolutionsAnswer>(response.Content, GPNJsonSerializer.Option());
                return (true, null, answer);
            }
            catch (Exception ex)
            {
                Log.Error("{source}: '{query}'. Error: '{error}'", "Apsolutions", Question, ex.Message);
                return (false, ex.Message, null);
            }
        }

        private static async Task<(bool, string, ApsolutionsAggregate)> SearchAggregate(Guid Id, string FIO, string Position, DateTime EmploymentDate, long ServiceID, string Question)
        {
            var (client, request) = GetRequest("https://semsearch.apsolutions.ru/api/v1/question/service_request");

            request.AddJsonBody(new
            {
                Profile = new
                {
                    FIO,
                    Id,
                    Position,
                    EmploymentDate
                },
                Question
            });

            try
            {
                var response = await client.ExecuteAsync(request).ConfigureAwait(false);
                var answer = JsonSerializer.Deserialize<ApsolutionsAggregate>(response.Content, GPNJsonSerializer.Option());
                return (true, null, answer);
            }
            catch (Exception ex)
            {
                Log.Error("{source}: '{query}'. Error: '{error}'", "Apsolutions", Question, ex.Message);
                return (false, ex.Message, null);
            }
        }


        public static async Task<(bool, string, Message)> FindReferenceDataByQuestion(Message source, long serviceId, User user, 
            UsersRepository users, MessageParams messageParams, DBPIParams dbPIParams)
        {
            // TODO must be 1
            var startTime = DateTime.Now;
            var question = source.Body;

            var lastMessageID = users.GetLastMessageID(user.GUID);
            var (success, error, apsolutionsAnswer) = await SearchText(user.GUID, user.Fio, user.Position, user.EmploymentDate, serviceId, question).ConfigureAwait(false);
            if (users.IsOlderThenLast(user.GUID, lastMessageID)) 
                return (false, error, null);
            // TODO if(service != null && apsolutionsAnswer.QuestionCategory == "information")
                
            if (!success || apsolutionsAnswer is null || apsolutionsAnswer.Records is null || apsolutionsAnswer.Records.Length == 0)
            {
                users.SetLastMessageID(user.GUID);
                return (false, null, Message.Answer(source, success ? messageParams.NotFound : messageParams.AIError, DateTime.Now.Ticks - startTime.Ticks));
            }

            var userMessages = new Queue<Message>();
            foreach (var record in apsolutionsAnswer.Records)
            {
                var body = record.Text;
                string description = null, title = null;
                ulong iid = 0;
                if(ulong.TryParse(body, out var pid))
                { 
                    var initiativeRepository = new InitiativeRepository(new ConnectionService(dbPIParams));
                    var (notFound, _, info) = await initiativeRepository.GetAsync(pid).ConfigureAwait(false);
                    if(notFound)
                    {
                        title = info.Problem.Length < 65 ? info.Problem : $"{info.Problem.Substring(0, 62)}...";
                        body = $"<div class=\"gpn-source\">Портал инициатив &#62; {info.Iid}</div><div class=\"gpn-title\">{title}</div><div class=\"gpn-shortDescription\">{info.Organization} {info.Status}</div>";
                        description = 
                        $"<div class=\"gpn-key\">Инициатива:</div><div class=\"gpn-value\">{info.Iid}</div>" +
                        $"<div class=\"gpn-key\">Создана:</div><div class=\"gpn-value\">{info.Created:dd.MM.yyyy}</div>" +
                        $"<div class=\"gpn-key\">Автор:</div><div class=\"gpn-value\">{info.Author}</div>" +
                        $"<div class=\"gpn-key\">Организация/подразделение:</div><div class=\"gpn-value\">{info.Organization}</div>" +
                        $"<div class=\"gpn-key\">Проблема:</div><div class=\"gpn-value\">{info.Problem}</div>" +
                        $"<div class=\"gpn-key\">Решение:</div><div class=\"gpn-value\">{info.Solution}</div>" +
                        $"<div class=\"gpn-key\">Статус:</div><div class=\"gpn-value\">{info.Status}</div>" +
                        $"<div class=\"gpn-key\">Количество комментариев:</div><div class=\"gpn-value\">{info.CommentsCount}</div>" +
                        $"<div class=\"gpn-key\">Дата последнего комментария:</div><div class=\"gpn-value\">{info.LastCommentDate ?? "-"}</div>" +
                        $"<div class=\"gpn-key\">Комментарий:</div><div class=\"gpn-value\">{info.LastCommentText ?? "-"}</div>";
                        iid = pid;
                    }
                }
                        
                var answer = Message.Answer(source, body);
                answer.Description = description;
                answer.Title = title;
                answer.Iid = iid;
                answer.CanClick = true;
                answer.Created = startTime;

                userMessages.Enqueue(answer);
            }

            users.SetMessages(user.GUID, QueueType.information, userMessages);
            users.SetLastMessageID(user.GUID);
            return (true, null, null);
        }


        public static async Task<(bool, string, Message)> FindServiceByQuestion(Message source, User user, 
            UsersRepository users, MessageParams messageParams)
        {
            var startTime = DateTime.Now;
            var question = source.Body;
            var lastMessageID = users.GetLastMessageID(user.GUID);
            var (success, error, apsolutionsAnswer) = await GetCategorized(user.GUID, user.Fio, user.Position, user.EmploymentDate, question).ConfigureAwait(false);
            if (users.IsOlderThenLast(user.GUID, lastMessageID)) 
                return (false, null, null);

            var answerHasError = !success || apsolutionsAnswer is null || apsolutionsAnswer.ServiceIDs is null || apsolutionsAnswer.QuestionCategories is null;
            var validServises = apsolutionsAnswer?.ServiceIDs?
                .Where(sa => sa.ServiceID != 0 && ServiceItemRepository.GetServiceItems().Exists(s => s.Id == sa.ServiceID));
            answerHasError = validServises is null || validServises.Count() == 0;

            if (answerHasError)
            {
                users.SetLastMessageID(user.GUID);
                return (true, error, Message.Answer(source, success ? messageParams.NotFound : messageParams.AIError, DateTime.Now.Ticks - startTime.Ticks));
            }

            Message firstAnswer = null;
            var userMessagesForService = new Queue<Message>();
            foreach (var apsolutionService in validServises)
            {
                var serviceItem = ServiceItemRepository.GetServiceItems().FirstOrDefault(s => s.Id == apsolutionService.ServiceID);
                var commands = CommandRepository.GetYesNo(source.ID);
                commands[0].CommandText = serviceItem.GUID.ToString();
                var answer = new Message
                {
                    SourceId = source.ID ?? 0,
                    UserGuid = source.UserGuid,
                    Iid = source.Iid,
                    Date = source.Date,
                    ServiceGuid = source.ServiceGuid,

                    Body = $"Используем '{serviceItem.Title}'?",

                    GUID = Guid.NewGuid(),
                    Command = 0,
                    IsFile = false,
                    Commands = commands,
                    Style = 1
                };

                if (firstAnswer is null)
                {
                    firstAnswer = answer;
                    firstAnswer.Date = firstAnswer.Date.AddTicks(DateTime.Now.Ticks - startTime.Ticks);
                }
                else
                    userMessagesForService.Enqueue(answer);
            }

            users.SetMessages(user.GUID, QueueType.service, userMessagesForService);

            var validCategories = apsolutionsAnswer?.QuestionCategories?
                .Where(qc => qc.QuestionCategory != 0 && QuestionCategoryRepository.GetQuestionCategories().Exists(s => s.Id == qc.QuestionCategory));

            var userMessagesForQuestionCategory = new Queue<Message>();
            foreach (var apsolutionsCategory in validCategories)
            {
                var answer = new Message
                {
                    SourceId = source.ID ?? 0,
                    UserGuid = source.UserGuid,
                    Iid = source.Iid,
                    Date = source.Date,
                    ServiceGuid = source.ServiceGuid,
                    QuestionCategory = apsolutionsCategory.QuestionCategory,

                    Body = "",
                    GUID = Guid.NewGuid(),
                };

                userMessagesForQuestionCategory.Enqueue(answer);
            }

            users.SetMessages(user.GUID, QueueType.category, userMessagesForQuestionCategory);
            users.SetLastMessageID(user.GUID);

            return (true, null, firstAnswer);
        }


        public static async Task<(bool, string, Message)> FindAggregateByQuestion(Message source, long serviceId, User user, 
            UsersRepository users, MessageParams messageParams)
        {
            // TODO must be 1
            var startTime = DateTime.Now;
            var question = source.Body;

            var lastMessageID = users.GetLastMessageID(user.GUID);
            var (success, error, apsolutionsAggregate) = await SearchAggregate(user.GUID, user.Fio, user.Position, user.EmploymentDate, serviceId, question).ConfigureAwait(false);
            if (users.IsOlderThenLast(user.GUID, lastMessageID)) return (false, error, null);

            var resultCount = apsolutionsAggregate?.Res?.Count;
            string body = resultCount.HasValue ? $"найдено {resultCount}" : null;

            var tiks = DateTime.Now.Ticks - startTime.Ticks;
            var answer = !success || apsolutionsAggregate is null || apsolutionsAggregate.Res is null || body is null ?
                Message.Answer(source, success ? messageParams.NotFound : messageParams.AIError, tiks) :
                Message.Answer(source, body, tiks);

            users.SetLastMessageID(user.GUID);
            return (true, null, answer);
        }
    }
}
