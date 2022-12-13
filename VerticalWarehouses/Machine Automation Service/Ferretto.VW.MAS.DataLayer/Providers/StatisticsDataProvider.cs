using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class StatisticsDataProvider : IStatisticsDataProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly DataLayerContext dataContext;

        private readonly IErrorsProvider errorProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<DataLayerService> logger;

        #endregion

        #region Constructors

        public StatisticsDataProvider(
            DataLayerContext dataContext,
            IErrorsProvider errorProvider,
            IBaysDataProvider baysDataProvider,
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.errorProvider = errorProvider ?? throw new ArgumentNullException(nameof(errorProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public bool AddInverterStatistics(double workingHours,
            double operationHours,
            double peakHeatSinkTemperature,
            double peakInsideTemperature,
            double averageRMSCurrent,
            double averageActivePower)
        {
            bool addNew = false;
            lock (this.dataContext)
            {
                var stat = this.dataContext.MachineStatistics
                    .Include(i => i.InverterStatistics)
                    .ToArray()
                    .LastOrDefault();
                stat.TotalInverterMissionTime = workingHours;
                stat.TotalInverterPowerOnTime = operationHours;
                var inverterStat = new InverterStatistics()
                {
                    AverageActivePower = averageActivePower,
                    AverageRMSCurrent = averageRMSCurrent,
                    DateTime = DateTimeOffset.UtcNow,
                    PeakHeatSinkTemperature = peakHeatSinkTemperature,
                    PeakInsideTemperature = peakInsideTemperature
                };
                var oldStat = stat.InverterStatistics.FirstOrDefault(s => s.DateTime.Year == inverterStat.DateTime.Year
                    && s.DateTime.DayOfYear == inverterStat.DateTime.DayOfYear
                    && s.DateTime.Hour == inverterStat.DateTime.Hour);
                if (oldStat is null)
                {
                    stat.InverterStatistics.Add(inverterStat);
                    addNew = true;
                }
                else
                {
                    oldStat.AverageActivePower = averageActivePower;
                    oldStat.AverageRMSCurrent = averageRMSCurrent;
                    oldStat.DateTime = DateTimeOffset.UtcNow;
                    oldStat.PeakHeatSinkTemperature = peakHeatSinkTemperature;
                    oldStat.PeakInsideTemperature = peakInsideTemperature;
                }
                this.dataContext.SaveChanges();
                this.logger.LogInformation($"Save inverter stat: workingHours {workingHours:0.00}; " +
                    $"operationHours {operationHours:0.00}; " +
                    $"averageActivePower {averageActivePower:0.00}; " +
                    $"averageRMSCurrent {averageRMSCurrent:0.00}; " +
                    $"peakHeatSinkTemperature {peakHeatSinkTemperature:0.00}; " +
                    $"peakInsideTemperature {peakInsideTemperature:0.00}; " +
                    $"new {addNew}; ");
            }
            return addNew;
        }

        public int ConfirmAndCreateNew()
        {
            lock (this.dataContext)
            {
                var s = new MachineStatistics();

                this.dataContext.MachineStatistics.Add(s);
                this.dataContext.SaveChanges();
            }

            return this.dataContext.MachineStatistics.AsNoTracking().Select(m => m.Id).ToArray().LastOrDefault();
        }

        public MachineStatistics GetActual()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.AsNoTracking().ToArray().LastOrDefault();
            }
        }

        public IEnumerable<MachineStatistics> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.AsNoTracking().ToList();
            }
        }

        public MachineStatistics GetById(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.Include(m => m.InverterStatistics).FirstOrDefault(s => s.Id == id);
            }
        }

        public MachineStatistics GetLastConfirmed()
        {
            lock (this.dataContext)
            {
                int dim = this.dataContext.MachineStatistics.ToArray().Count();

                if (dim > 1)
                {
                    return this.dataContext.MachineStatistics.ToArray().ElementAt(dim - 1);
                }
                else
                {
                    return null;
                }
            }
        }

        public int MissionTotalNumber()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfo.AsNoTracking().Select(s => s.TotalMissions).ToArray().LastOrDefault() ?? 0;
            }
        }

        public int PurgeInverterStatistics()
        {
            lock (this.dataContext)
            {
                var count = 0;
                var old = this.dataContext.InverterStatistics
                    .AsEnumerable()
                    .Where(x => DateTimeOffset.UtcNow.Subtract(x.DateTime).Days > 15)
                    .ToList();
                if (old.Any())
                {
                    count = old.Count;
                    this.dataContext.InverterStatistics.RemoveRange(old);
                    this.dataContext.SaveChanges();
                    this.logger.LogInformation($"Deleted {count} InverterStatistics.");
                }
                return count;
            }
        }

        public double TotalDistance()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.AsNoTracking().Select(s => s.TotalVerticalAxisKilometers).ToArray().LastOrDefault();
            }
        }

        #endregion
    }
}
