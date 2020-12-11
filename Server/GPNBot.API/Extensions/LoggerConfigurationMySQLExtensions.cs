using System;
using Serilog.Configuration;
using Serilog.Events;

using GPNBot.API.Tools;
using GPNBot.API.Services;
using GPNBot.API.Models;


namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.Stackify() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationMySQLExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to the elmah.io webservice.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="dbLogParams">Log Database server parameters.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink. Set to Verbose by default.</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MySQL(
            this LoggerSinkConfiguration loggerConfiguration,
            DBLogParams dbLogParams,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum
            )
        {
            if (loggerConfiguration == null) throw new ArgumentNullException("loggerConfiguration");

            return loggerConfiguration
                .Sink(new MySQLSink(dbLogParams), restrictedToMinimumLevel);
        }
    }
}