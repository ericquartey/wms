using System;
using System.Data.Common;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DiagnosticAdapter;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public class CommandListener
    {
        #region Fields

        private readonly DataLayerContext dataLayerContext;

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        #endregion

        #region Constructors

        public CommandListener(
            DataLayerContext dataLayerContext,
            IDbContextRedundancyService<DataLayerContext> redundancyService)
        {
            if (dataLayerContext == null)
            {
                throw new ArgumentNullException(nameof(dataLayerContext));
            }

            if (redundancyService == null)
            {
                throw new ArgumentNullException(nameof(redundancyService));
            }

            this.dataLayerContext = dataLayerContext;
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
            this.redundancyService.HandleDbContextFault(this.dataLayerContext, exception);
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.ConnectionError")]
        public void OnConnectionError(
             Microsoft.EntityFrameworkCore.Storage.IRelationalConnection connection,
             Exception exception,
             DateTimeOffset startTime,
             TimeSpan duration,
             bool async,
             bool logErrorAsDebug)
        {
            this.redundancyService.HandleDbContextFault(this.dataLayerContext, exception);
        }

        #endregion
    }
}
