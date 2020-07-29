﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IMissionLogProvider
    {
        #region Methods

        void DeleteOldLogs(TimeSpan maximumLogTimespan);

        IEnumerable<IMissionLog> GetAll();

        Task SaveAsync(string serialNumber, IMissionLog mission);

        #endregion
    }
}
