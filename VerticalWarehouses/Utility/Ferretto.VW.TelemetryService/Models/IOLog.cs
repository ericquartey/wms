﻿using System;
using System.ComponentModel.DataAnnotations;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class IOLog : DataModel, IIOLog
    {
        #region Properties

        public int BayNumber { get; set; }

        public string? Description { get; set; }

        public string? Input { get; set; }

        public Machine? Machine { get; set; }

        public string? Output { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        #endregion
    }
}
