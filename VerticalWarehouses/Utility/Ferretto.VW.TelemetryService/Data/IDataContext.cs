using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.TelemetryService.Data
{
    public interface IDataContext : System.IDisposable
    {
        #region Properties

        DbSet<ErrorLog> ErrorLogs { get; set; }

        DbSet<IOLog> IOLogs { get; set; }

        DbSet<Machine> Machines { get; set; }

        DbSet<MissionLog> MissionLogs { get; set; }

        DbSet<ScreenShot> ScreenShots { get; set; }

        DbSet<ServicingInfo> ServicingInfos { get; set; }

        #endregion

        #region Methods

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        #endregion
    }
}
