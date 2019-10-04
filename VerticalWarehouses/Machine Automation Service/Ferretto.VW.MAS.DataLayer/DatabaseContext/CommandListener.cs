using System;
using System.Data.Common;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public class CommandListener<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Fields

        private readonly ILogger<DbContext> logger;

        private readonly IDbContextRedundancyService<TDbContext> redundancyService;

        private bool writingOnStandby;

        #endregion

        #region Constructors

        public CommandListener(
            IDbContextRedundancyService<TDbContext> redundancyService,
            ILogger<DataLayerContext> logger)
        {
            this.redundancyService = redundancyService ?? throw new ArgumentNullException(nameof(redundancyService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        public void OnCommandError(
                DbCommand command,
                DbCommandMethod executeMethod,
                Guid commandId,
                Guid connectionId,
                Exception exception,
                bool async,
                DateTimeOffset startTime,
                TimeSpan duration)
        {
            this.logger.LogError($"Database command error.");
            this.OnCommandOrConnectionError(exception);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(
            DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
        {
            lock (this.redundancyService)
            {
                if (this.redundancyService.IsEnabled
                    &&
                    IsModifyingCommand(command)
                    &&
                    this.IsActiveDbChannel(command.Connection.ConnectionString)
                    &&
                    !this.redundancyService.IsStandbyDbInhibited
                    &&
                    !this.writingOnStandby)
                {
                    this.writingOnStandby = true;

                    using (var dbContext = new DataLayerContext(isActiveChannel: false, this.redundancyService as IDbContextRedundancyService<DataLayerContext>))
                    {
                        var parametersArray = new SqliteParameter[command.Parameters.Count];
                        command.Parameters.CopyTo(parametersArray, 0);

                        try
                        {
                            dbContext.Database.ExecuteSqlCommand(command.CommandText, parametersArray);
                        }
                        catch
                        {
                            this.redundancyService.InhibitStandbyDb();
                        }

                        this.writingOnStandby = false;
                    }
                }
            }
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionError")]
        public void OnConnectionError(
               DbCommand command,
               DbCommandMethod executeMethod,
               Guid commandId,
               Guid connectionId,
               Exception exception,
               bool async,
               DateTimeOffset startTime,
               TimeSpan duration)
        {
            this.logger.LogError($"Database connection error.");
            this.OnCommandOrConnectionError(null);
        }

        private static bool IsModifyingCommand(DbCommand command)
        {
            var normalizedCommandText = command.CommandText.ToUpperInvariant();

            return
                normalizedCommandText.Contains("UPDATE ")
                ||
                normalizedCommandText.Contains("INSERT ")
                ||
                normalizedCommandText.Contains("DELETE ");
        }

        private bool IsActiveDbChannel(string connectionString)
        {
            var activeDbExtension = this.redundancyService.ActiveDbContextOptions.FindExtension<SqliteOptionsExtension>();
            return connectionString == activeDbExtension.ConnectionString;
        }

        private void OnCommandOrConnectionError(Exception exception)
        {
            lock (this.redundancyService)
            {
                var dbContextOptions = this.writingOnStandby
                ? this.redundancyService.StandbyDbContextOptions
                : this.redundancyService.ActiveDbContextOptions;

                if (this.redundancyService.IsEnabled)
                {
                    this.redundancyService.HandleDbContextFault(dbContextOptions, exception);
                }
            }
        }

        #endregion
    }
}
