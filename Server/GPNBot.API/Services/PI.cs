using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

using RestSharp;

using GPNBot.API.Tools;


namespace GPNBot.API.Services
{
    public static class PI
    {
        public static async Task<(bool, string, string)> GetInitiativeStatusAsync(IMemoryCache memoryCache, ulong initiative)
        {
            var (successToken, error, token) = await GetTokenAsync(memoryCache).ConfigureAwait(false);
            if (!successToken)
                return (successToken, error, null);


            var (client, request) = GetRequest("https://pi.gazprom-neft.ru/api/get/initiative/", token);
            request.AddParameter("id", initiative);

            try
            {
                var response = await client.ExecuteAsync(request).ConfigureAwait(false);
                var gpnIStatus = JsonSerializer.Deserialize<GpnIStatus>(response.Content, GPNJsonSerializer.Option());
                if (!gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, null, gpnIStatus.Error);
                
                return (true, null, $"Статус инициативы {initiative} '{gpnIStatus.Data.Initiative.STATUS}'");
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<(bool, string, string)> GetInitiativeListAsync(IMemoryCache memoryCache, string email)
        {
            var (successToken, error, token) = await GetTokenAsync(memoryCache).ConfigureAwait(false);
            if (!successToken)
                return (successToken, error, null);


            var (client, request) = GetRequest("https://pi.gazprom-neft.ru/api/get/author_initiatives/", token);
            request.AddParameter("email", email);

            try
            {
                var response = await client.ExecuteAsync(request).ConfigureAwait(false);

                var gpnIList = JsonSerializer.Deserialize<GpnIList>(response.Content, GPNJsonSerializer.Option());
                if (!gpnIList.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, gpnIList.Error, null);

                if (gpnIList.Data.Initiatives is null)
                    return (false, null, "Нет инициатив");

                var body = "Список инициатив:\n";
                foreach (var key in gpnIList.Data.Initiatives.Keys)
                    body += $"{key}: '{gpnIList.Data.Initiatives[key]}'\n";

                return (true, null, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<(bool, string, string)> GetInitiativeActivityAsync(IMemoryCache memoryCache, ulong initiative)
        {
            var (successToken, error, token) = await GetTokenAsync(memoryCache).ConfigureAwait(false);
            if (!successToken)
                return (successToken, error, null);


            var (client, request) = GetRequest("https://pi.gazprom-neft.ru/api/get/initiative/", token);
            request.AddParameter("id", initiative);

            try
            {
                var body = $"Активность по инициативе {initiative}'\n";
    
                request.AddParameter("property", "DATE");
                var (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                body += $"Дата подачи: '{gpnIStatus.Data.Initiative.DATE}'\n";

                request.Parameters.Last().Value = "COMMENTS_COUNT";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                if (!string.IsNullOrEmpty(gpnIStatus.Data.Initiative.COMMENTS_COUNT) &&
                    int.TryParse(gpnIStatus.Data.Initiative.COMMENTS_COUNT, out int commentCount))
                    body += $"Количество комментариев: '{commentCount}'\n";

                request.Parameters.Last().Value = "LAST_COMMENT";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                var lc = gpnIStatus.Data.Initiative.LAST_COMMENT;
                body += $"Последний комментарий: '{(lc is null ? "-" : $"{lc.DATE}: {lc.TEXT}")}'";

                return (true, null, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public static async Task<(bool, string, string)> GetInitiativeExportAsync(IMemoryCache memoryCache, ulong initiative)
        {
            var (successToken, error, token) = await GetTokenAsync(memoryCache).ConfigureAwait(false);
            if (!successToken)
                return (successToken, error, null);


            var (client, request) = GetRequest("https://pi.gazprom-neft.ru/api/get/initiative/", token);
            request.AddParameter("id", initiative);

            try
            {
                var gpnI = new GpnIStatus();
                gpnI.Data.Initiative.ID = initiative.ToString();

                var (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                
                gpnI.Data.Initiative.STATUS = gpnIStatus.Data.Initiative.STATUS;


                request.AddParameter("property", "DATE");
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.DATE = gpnIStatus.Data.Initiative.DATE;

                request.Parameters.Last().Value = "COMMENTS_COUNT";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.COMMENTS_COUNT = gpnIStatus.Data.Initiative.COMMENTS_COUNT;

                request.Parameters.Last().Value = "LAST_COMMENT";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.LAST_COMMENT = gpnIStatus.Data.Initiative.LAST_COMMENT;

                request.AddParameter("property", "AUTHOR");
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.AUTHOR = gpnIStatus.Data.Initiative.AUTHOR;

                // -- COAUTHORS - Соавторы инициативы

                request.Parameters.Last().Value = "ORGANIZATION";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.ORGANIZATION = gpnIStatus.Data.Initiative.ORGANIZATION;

                request.Parameters.Last().Value = "PROBLEM";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.PROBLEM = gpnIStatus.Data.Initiative.PROBLEM;

                request.Parameters.Last().Value = "SOLUTION";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);

                gpnI.Data.Initiative.SOLUTION = gpnIStatus.Data.Initiative.SOLUTION;

                var body = JsonSerializer.Serialize(gpnI.Data.Initiative, GPNJsonSerializer.Option());
                return (true, null, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }


        public static async Task<(bool, string, string)> GetInitiativeDescriptionAsync(IMemoryCache memoryCache, ulong initiative)
        {
            var (successToken, error, token) = await GetTokenAsync(memoryCache).ConfigureAwait(false);
            if (!successToken)
                return (successToken, error, null);


            var (client, request) = GetRequest("https://pi.gazprom-neft.ru/api/get/initiative/", token);
            request.AddParameter("id", initiative);

            try
            {
                var body = $"Описание инициативы {initiative}'\n";

                request.AddParameter("property", "AUTHOR");
                var (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                body += $"Автор инициативы: '{gpnIStatus.Data.Initiative.AUTHOR}'\n";

                // -- COAUTHORS - Соавторы инициативы

                request.Parameters.Last().Value = "ORGANIZATION";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                body += $"ДО: '{gpnIStatus.Data.Initiative.ORGANIZATION}'\n";

                request.Parameters.Last().Value = "PROBLEM";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                body += $"Проблема: '{gpnIStatus.Data.Initiative.PROBLEM}'";

                request.Parameters.Last().Value = "SOLUTION";
                (success, message, gpnIStatus) = await GetGpnIStatusData(client, request).ConfigureAwait(false);
                if (!success || !gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                    return (false, success ? gpnIStatus.Error : message, null);
                body += $"Решение: '{(string.IsNullOrEmpty(gpnIStatus.Data.Initiative.SOLUTION) ? "-" : gpnIStatus.Data.Initiative.SOLUTION)}'";

                return (true, null, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        private static (RestClient, RestRequest) GetRequest(string url, string token)
        {
            var client = new RestClient(url){ Timeout = -1 };
            var request = new RestRequest(Method.POST);
            request.AddHeader("Cookie", "");
            request.AddParameter("token", token);

            return (client, request);
        }

        private static async Task<(bool, string, GpnIStatus)> GetGpnIStatusData(RestClient client, RestRequest request)
        {
            var response = await client.ExecuteAsync(request).ConfigureAwait(false);
            var gpnIStatus = JsonSerializer.Deserialize<GpnIStatus>(response.Content, GPNJsonSerializer.Option());
            if (!gpnIStatus.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                return (false, gpnIStatus.Error, null);

            return (true, null, gpnIStatus);
        }

        private static async Task<(bool, string, string)> GetTokenAsync(IMemoryCache memoryCache)
        {
            var cacheKey = "PI-chat-bot";
            if(!memoryCache.TryGetValue(cacheKey, out string token))
            {
                try
                {
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddHeader("Cookie", "");
                    request.AddParameter("login", "");
                    request.AddParameter("password", "");

                    var client = new RestClient("https://pi.gazprom-neft.ru/api/get/token/"){ Timeout = -1 };
                    var response = await client.ExecuteAsync(request).ConfigureAwait(false);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        return (false, "Не удалось получить токен", null);

                    var gpnToken = JsonSerializer.Deserialize<GpnToken>(response.Content, GPNJsonSerializer.Option());
                    if (!gpnToken.Result.Equals("OK", StringComparison.InvariantCultureIgnoreCase))
                        return (false, gpnToken.Data.Token, null);

                    memoryCache.Set(cacheKey, gpnToken.Data.Token, DateTimeOffset.Now.AddMinutes(15));
                
                    return (true, null, gpnToken.Data.Token);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message, null);
                }
            }
            
            return (true, null, token);
        }
    }


    public class GpnToken
    {

        public string Result { get; set; }      // "OK",

        public TokenValue Data { get; set; }    //"data": { "token": "3ae491c40f22180a72e3f9e8c804409fcb664881" }   

        public class TokenValue
        {
            public string Token { get; set; }
        }
    }

    public class GpnIStatus
    {
        public GpnIStatus()
        {
            Data = new StatusValue();
        }


        public string Result { get; set; }      // "OK",
        public string Error { get; set; }

        public StatusValue Data { get; set; }    //"data": { "title": "Статус инициативы", "initiative": { "ID": "10207", "STATUS": "2 этап" }}

        public class StatusValue
        {
            public StatusValue()
            {
                Initiative = new InitiativeValue();
            }

            public string Title { get; set; }

            public InitiativeValue Initiative { get; set; }

            public class InitiativeValue
            {
                public InitiativeValue()
                {
                    LAST_COMMENT = new LastComment();
                }

                public string ID { get; set; }
                public string STATUS { get; set; }

                public string DATE { get; set; }
                public string COMMENTS_COUNT { get; set; }
                public LastComment LAST_COMMENT { get; set; }

                public string AUTHOR { get; set; }
                public string COAUTHORS { get; set; }
                public string ORGANIZATION { get; set; }
                public string PROBLEM { get; set; }
                public string SOLUTION { get; set; }


                public class LastComment
                {
                    public string DATE { get; set; }

                    public string TEXT { get; set; }

                }
            }
        }
    }

    public class GpnIList
    {
        public string Result { get; set; }      // "OK",
        public string Error { get; set; }

        public ListValue Data { get; set; }    //"data":{"title":"Инициативы автора","initiatives":{"10207":"2 этап","7369":"Рекомендована к реализации","6831":"Требуется отклонение"}}}

        public class ListValue
        {
            public string Title { get; set; }

            public Dictionary<string, string> Initiatives { get; set; }
        }
    }
}
