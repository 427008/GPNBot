using System;
using System.Text.Json;

using Serilog.Core;
using Serilog.Events;

using GPNBot.API.Models;
using GPNBot.API.Repositories;
using GPNBot.API.Services;


namespace GPNBot.API.Tools
{
    public class MySQLSink : ILogEventSink
    {
        private readonly DBLogParams _dbLogParams;
        /// <summary>
        /// Construct a sink that saves logs to the specified storage account.
        /// </summary>
        public MySQLSink(DBLogParams dbLogParams)
        {
            _dbLogParams = dbLogParams ?? throw new ArgumentNullException(nameof(dbLogParams));
        }


        /// <summary>
        /// Emit the provided log event to the sink.
        /// </summary>
        /// <param name="logEvent">The log event to write.</param>
        public void Emit(LogEvent logEvent)
        {
            if(logEvent.Properties.TryGetValue("source", out var soursePropery))
            { 
                
                var source = soursePropery?.ToString() ?? "";
                if(source.Contains("controller", StringComparison.InvariantCultureIgnoreCase))
                { 
                    logEvent.Properties.TryGetValue("RequestPath", out var pathPropery);
                    logEvent.Properties.TryGetValue("action", out var actionPropery);
                    logEvent.Properties.TryGetValue("authorization", out var authorizationPropery);
                    
                    var info = new { 
                        path = pathPropery?.ToString().Trim('"') ?? "",
                        action = actionPropery?.ToString().Trim('"') ?? "",
                        authorization = authorizationPropery?.ToString().Trim('"') ?? "",
                        data = logEvent.RenderMessage().Trim('"')
                    };

                    logEvent.Properties.TryGetValue("remoteIp", out var addressPropery);

                    var msgFromSource = new ApiLogWeb
                    {
                        Level = logEvent.Level.ToString(),
                        Address = addressPropery?.ToString().Trim('"') ?? "-",
                        Data = JsonSerializer.Serialize(info, GPNJsonSerializer.Option())
                    };
                    Save(msgFromSource, new ConnectionService(_dbLogParams));
                    return;
                }
                else if(source.Contains("repository", StringComparison.InvariantCultureIgnoreCase))
                { 
                    logEvent.Properties.TryGetValue("watch", out var watchPropery);
                    logEvent.Properties.TryGetValue("query", out var queryPropery);
                    logEvent.Properties.TryGetValue("parameters", out var parametersPropery);

                    var info = new { 
                        watch = watchPropery?.ToString().Trim('"') ?? "",
                        query = queryPropery?.ToString().Trim('"') ?? "",
                        parameters = parametersPropery?.ToString().Trim('"') ?? "",
                        data = logEvent.RenderMessage().Trim('"')
                    };

                    var msgFromSource = new ApiLogWeb
                    {
                        Level = logEvent.Level.ToString(),
                        Address = source.Trim('"') ?? "-",
                        Data = JsonSerializer.Serialize(info, GPNJsonSerializer.Option())
                    };
                    Save(msgFromSource, new ConnectionService(_dbLogParams));
                    return;
                }
                else if(source.Contains("apsolutions", StringComparison.InvariantCultureIgnoreCase))
                { 
                    logEvent.Properties.TryGetValue("watch", out var watchPropery);
                    logEvent.Properties.TryGetValue("query", out var queryPropery);
                    logEvent.Properties.TryGetValue("parameters", out var parametersPropery);

                    var info = new { 
                        watch = watchPropery?.ToString().Trim('"') ?? "",
                        query = queryPropery?.ToString().Trim('"') ?? "",
                        parameters = parametersPropery?.ToString().Trim('"') ?? "",
                        data = logEvent.RenderMessage().Trim('"')
                    };

                    var msgFromSource = new ApiLogWeb
                    {
                        Level = logEvent.Level.ToString(),
                        Address = source.Trim('"') ?? "-",
                        Data = JsonSerializer.Serialize(info, GPNJsonSerializer.Option())
                    };
                    Save(msgFromSource, new ConnectionService(_dbLogParams));
                    return;
                }
                
            }
            var msg = new ApiLogWeb
            {
                Level = logEvent.Level.ToString(),
                Address = "",
                Data = logEvent.Level != LogEventLevel.Error ? 
                    logEvent.RenderMessage() : 
                    logEvent.RenderMessage() + "\n" + logEvent.Exception?.Message ?? "unknown"  + "\n" + logEvent.Exception?.StackTrace ?? "no trace",
            };

            Save(msg, new ConnectionService(_dbLogParams));
        }

        private async void Save(ApiLogWeb msg, IConnectionService connectionService )
        {
            try
            {
                var apiLogWebRepository = new ApiLogWebRepository(connectionService);
                await apiLogWebRepository.PostAsync(msg).ConfigureAwait(false);
            }
            catch{ }
        }
    }
}
