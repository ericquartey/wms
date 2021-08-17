using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class CommandListener<TDbContext>
        where TDbContext : DbContext, IRedundancyDbContext<TDbContext>
    {
        #region Fields

        private readonly ILogger<DataLayerContext> logger;

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

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Remove unused parameter

        //[DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandError")]
        //public void OnCommandError(
        //        DbCommand command,
        //        DbCommandMethod executeMethod,
        //        Guid commandId,
        //        Guid connectionId,
        //        Exception exception,
        //        bool async,
        //        DateTimeOffset startTime,
        //        TimeSpan duration)
        //{
        //    lock (this.redundancyService)
        //    {
        //        if (this.redundancyService.IsEnabled)
        //        {
        //            this.logger.LogError($"Database command error.");
        //            this.OnCommandOrConnectionError(exception);
        //        }
        //    }
        //}

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuting")]
        public void OnCommandExecuting(
            DbCommand command,
            DbCommandMethod executeMethod,
            Guid commandId,
            Guid connectionId,
            bool async,
            DateTimeOffset startTime)
        {
            //#if DEBUG
            //            var stackTrace = new System.Diagnostics.StackTrace();
            //            var methods = new List<string>();
            //            for (var i = 1; i < stackTrace.FrameCount && methods.Count < 4; i++)
            //            {
            //                var frame = stackTrace.GetFrame(i);
            //                var method = frame.GetMethod();
            //                if (method.Module.Name.Contains("Ferretto", StringComparison.InvariantCultureIgnoreCase))
            //                {
            //                    methods.Add($"{method.Module.Name}!{method.Name}");
            //                }
            //            }
            //            methods.Reverse();

            //            this.logger.LogTrace($"{string.Join(" -> ", methods)}\n{command.CommandText}");
            //#else
            this.logger.LogTrace($"{command.CommandText}");
            //#endif

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
                        catch (Exception ex)
                        {
                            //this.logger.LogWarning("Inhibiting database standby channel.");
                            //this.redundancyService.InhibitStandbyDb();
                            this.logger.LogError($"Error writing to standby database: {ex.Message} {command.CommandText.Substring(0, 100)}");
                            this.writingOnStandby = false;

                            // TODO - please enable the following instruction to make standby database errors blocking
                            //throw new InvalidOperationException($"Error writing to standby database: {ex.Message}");
                        }

                        this.writingOnStandby = false;
                    }
                }
            }
        }

        //[DiagnosticName("Microsoft.EntityFrameworkCore.Database.Connection.ConnectionError")]
        //public void OnConnectionError(
        //       DbCommand command,
        //       DbCommandMethod executeMethod,
        //       Guid commandId,
        //       Guid connectionId,
        //       Exception exception,
        //       bool async,
        //       DateTimeOffset startTime,
        //       TimeSpan duration)
        //{
        //    lock (this.redundancyService)
        //    {
        //        if (this.redundancyService.IsEnabled)
        //        {
        //            this.logger.LogError($"Database connection error.");
        //            this.OnCommandOrConnectionError(null);
        //        }
        //    }
        //}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CA1801 // Remove unused parameter

        private static bool IsModifyingCommand(DbCommand command)
        {
            return
                command.CommandText.Contains("UPDATE ", StringComparison.InvariantCultureIgnoreCase)
                ||
                command.CommandText.Contains("INSERT ", StringComparison.InvariantCultureIgnoreCase)
                ||
                command.CommandText.Contains("DELETE ", StringComparison.InvariantCultureIgnoreCase);
        }

        private bool IsActiveDbChannel(string connectionString)
        {
            lock (this.redundancyService)
            {
                var activeDbExtension = this.redundancyService.ActiveDbContextOptions.FindExtension<SqliteOptionsExtension>();
                return connectionString == activeDbExtension.ConnectionString;
            }
        }

        private void OnCommandOrConnectionError(Exception exception)
        {
            // DO NOTHING!! please never swap a database by software

            //lock (this.redundancyService)
            //{
            //    var dbContextOptions = this.writingOnStandby
            //    ? this.redundancyService.StandbyDbContextOptions
            //    : this.redundancyService.ActiveDbContextOptions;

            //    if (this.redundancyService.IsEnabled)
            //    {
            //        this.redundancyService.HandleDbContextFault(dbContextOptions, exception);
            //    }
            //}
        }

        #endregion
    }
}
