using System;
using System.Collections.Generic;
using System.Data.Common;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public class CommandListener
    {
        #region Fields

        private readonly IDictionary<DataLayerContext, bool> contexts
            = new Dictionary<DataLayerContext, bool>();

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        private bool writingOnStandby;

        #endregion

        #region Constructors

        public CommandListener(
            IDbContextRedundancyService<DataLayerContext> redundancyService)
        {
            if (redundancyService == null)
            {
                throw new ArgumentNullException(nameof(redundancyService));
            }

            this.redundancyService = redundancyService;
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
            var dbContextOptions = !this.writingOnStandby
                ? this.redundancyService.ActiveDbContextOptions
                : this.redundancyService.StandbyDbContextOptions;

            lock (this.redundancyService)
            {
                this.redundancyService.HandleDbContextFault(dbContextOptions, exception);
            }
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
                if (IsInsertOrUpdateCommand(command)
                    &&
                    this.IsActiveDbChannel(command.Connection.ConnectionString)
                    &&
                    !this.redundancyService.IsStandbyDbInhibited
                    &&
                    !this.writingOnStandby)
                {
                    this.writingOnStandby = true;

                    var dbContext = new DataLayerContext(
                       isActiveChannel: false,
                       this.redundancyService);

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

        internal void RegisterInstance(DataLayerContext dataLayerContext, bool isActiveChannel)
        {
            this.contexts.Add(dataLayerContext, isActiveChannel);
        }

        private static bool IsInsertOrUpdateCommand(DbCommand command)
        {
            var normalizedCommandText = command.CommandText.ToUpperInvariant();

            return
                normalizedCommandText.Contains("UPDATE ")
                ||
                normalizedCommandText.Contains("INSERT ");
        }

        private bool IsActiveDbChannel(string connectionString)
        {
            var activeDbExtension = this.redundancyService.ActiveDbContextOptions.FindExtension<SqliteOptionsExtension>();
            return connectionString == activeDbExtension.ConnectionString;
        }

        #endregion
    }
}
