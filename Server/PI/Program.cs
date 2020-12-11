using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

using GPNBot.API.Tools;
using GPNBot.API.Services;
using GPNBot.API.Models;

namespace PIImport
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var provider = new ServiceCollection()
                       .AddMemoryCache()
                       .BuildServiceProvider();
            
            var cache = provider.GetService<IMemoryCache>();
            var dbPIParams = new DBPIParams() { ConnectionString = "server=127.0.0.1;port=3307;uid=gpnadmin;pwd=8FxLWcquHb;database=pidump" };
            var connectionService = new ConnectionService(dbPIParams);

            var errorNo = 0;
            for(ulong i = 10217; i < 15000; i ++)
            {
                var (success, error, body) = await PI.GetInitiativeExportAsync(cache, i).ConfigureAwait(false);
                errorNo = success ? 0 : errorNo+1;
                if(errorNo > 100) break;
                await Task.Delay(500);

                try
                {
                    var initiative = success ?
                        JsonSerializer.Deserialize<Initiative>(body, GPNJsonSerializer.Option()) :
                        new Initiative() { Iid = i };

                    var apiLogWebRepository = new InitiativeRepository(connectionService);
                    (success, _, _) = await apiLogWebRepository.PostAsync(initiative).ConfigureAwait(false);
                    if(!success)
                    { 
                        ;
                    }
                }
                catch (Exception ex)
                {
                    ;
                }
            }
        }
    }
}
