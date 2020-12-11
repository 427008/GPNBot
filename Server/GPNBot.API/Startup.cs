using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Dapper;
using Serilog;
using Serilog.Events;

using GPNBot.API.Models;
using GPNBot.API.Services;
using GPNBot.API.Tools;
using GPNBot.API.Repositories;
using GPNBot.API.Commands;
using GPNBot.API.WS;
using GPNBot.API.WS.Model;


namespace GPNBot.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("allowAll",
                builder =>
                {
                    // Not a permanent solution, but just trying to isolate the problem
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            
            services.AddMemoryCache();
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = GPNJsonSerializer.Option().IgnoreNullValues;
                    options.JsonSerializerOptions.PropertyNamingPolicy = GPNJsonSerializer.Option().PropertyNamingPolicy;
                    options.JsonSerializerOptions.WriteIndented = GPNJsonSerializer.Option().WriteIndented;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = GPNJsonSerializer.Option().PropertyNameCaseInsensitive;
                });

            services.Configure<MessageParams>(Configuration.GetSection("MessageParams"));
            var dialog = Configuration.GetSection("Dialog").Get<Dialog>();
            foreach(var template in dialog.Templates)
            { 
                template.Command = 6;
                if(template.Commands is null) template.Commands = new List<CommandItem>();
                template.Commands.ForEach(c => {
                    c.Template = c.Title;
                    if(!string.IsNullOrEmpty(c.Email))
                    {
                        c.Code = 5;
                        var l = new List<string>() { c.CommandText ?? "" };
                        l.Add(c.Email);
                        c.CommandText = string.Join(";", l);
                        c.Email = null;
                    }
                    else if(!string.IsNullOrEmpty(c.Action))
                    {
                        c.Code = 7;
                        c.CommandText = $"{c.CommandText ?? ""};{c.Action}";
                        c.Action = null;
                    }
                    else
                        c.Code = 6;
                });
            }
            services.Configure<Dialog>(options => { options.Templates = dialog.Templates; });

            services.Configure<DBServerParams>(Configuration.GetSection("DBServerParams"));
            services.AddTransient<IConnectionService, ConnectionService>();
            services.AddSingleton<UsersRepository>();

            var dbLogParams = Configuration.GetSection("DBLogParams").Get<DBLogParams>();
            var logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.MySQL(dbLogParams)
                .CreateLogger();

            Log.Logger = logger;

            services.Configure<DBPIParams>(Configuration.GetSection("DBPIParams"));

            SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));

            // TODO WS
            services.AddSingleton<WebSocketConnectionManager>();
            services.AddTransient<WebSocketHandler>();
            
            services.AddSingleton<CommnadExecutor>();
            services.AddScoped<AnswerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // TODO WS
            app.UseWebSockets();

            app.UseRouting();

            app.UseCors("allowAll");

            app.UseAuthorization();

            app.UseMiddleware<TokenMiddleware>();
            app.UseMiddleware<WebSocketManagerMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
