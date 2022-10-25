using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class AutoCompactingSettingsProvider : BaseProvider, IAutoCompactingSettingsProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly DataLayerContext dataContext;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<DataLayerService> logger;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public AutoCompactingSettingsProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IMissionsDataProvider missionsDataProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IBaysDataProvider baysDataProvider,
            ILogger<DataLayerService> logger) : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void AddAutoCompactingSettings(AutoCompactingSettings autoCompactingSettings)
        {
            lock (this.dataContext)
            {
                //logoutSettings.RemainingTime = logoutSettings.Timeout;
                this.dataContext.AutoCompactingSettings.Add(autoCompactingSettings);
                this.dataContext.SaveChanges();
            }
        }

        public void AddOrModifyAutoCompactingSettings(AutoCompactingSettings autoCompactingSettings)
        {
            lock (this.dataContext)
            {
                if (!this.dataContext.AutoCompactingSettings.Any(s => s.Id == autoCompactingSettings.Id))
                {
                    this.AddAutoCompactingSettings(autoCompactingSettings);
                }
                else
                {
                    this.ModifyAutoCompactingSettings(autoCompactingSettings);
                }
            }
        }

        public IEnumerable<AutoCompactingSettings> GetAllAutoCompactingSettings()
        {
            lock (this.dataContext)
            {
                var result = this.dataContext.AutoCompactingSettings.AsNoTracking();
                return result;
            }
        }

        public void ModifyAutoCompactingSettings(AutoCompactingSettings newAutoCompactingSettings)
        {
            lock (this.dataContext)
            {
                if (newAutoCompactingSettings != null)
                {
                    var autoCompactingSettings = this.dataContext.AutoCompactingSettings.Single(s => s.Id == newAutoCompactingSettings.Id);
                    autoCompactingSettings.IsActive = newAutoCompactingSettings.IsActive;
                    autoCompactingSettings.BeginTime = newAutoCompactingSettings.BeginTime;
                    autoCompactingSettings.IsOptimizeRotationClass = newAutoCompactingSettings.IsOptimizeRotationClass;
                    this.dataContext.AutoCompactingSettings.Update(autoCompactingSettings);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void RemoveAutoCompactingSettingsById(int id)
        {
            lock (this.dataContext)
            {
                var removeElement = this.dataContext.AutoCompactingSettings.Single(s => s.Id == id);
                this.dataContext.AutoCompactingSettings.Remove(removeElement);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateStatus()
        {
            try
            {
                var activeSettings = this.GetAllAutoCompactingSettings().ToList().Find(x => x.IsActive && DateTime.Now.TimeOfDay.TotalMinutes - x.BeginTime.TotalMinutes >= 0 && DateTime.Now.TimeOfDay.TotalMinutes - x.BeginTime.TotalMinutes <= 1);

                if (activeSettings != null)
                {
                    if (this.CanCompactingStart())
                    {
                        this.logger.LogDebug($"Auto Compacting Settings : Time {DateTime.Now.TimeOfDay.Hours}:{DateTime.Now.TimeOfDay.Minutes} : sort {activeSettings.IsOptimizeRotationClass} :  Compacting allowed.");
                        this.machineVolatileDataProvider.Mode = CommonUtils.Messages.MachineMode.Compact;
                        this.machineVolatileDataProvider.IsOptimizeRotationClass = activeSettings.IsOptimizeRotationClass;
                    }
                    else
                    {
                        this.logger.LogDebug($"Auto Compacting Settings : Time {DateTime.Now.TimeOfDay.Hours}:{DateTime.Now.TimeOfDay.Minutes} : Compacting denied.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"AutoCompactingSettings : {ex.Message}");
            }
        }

        private bool CanCompactingStart()
        {
            var canStart = this.machineVolatileDataProvider.Mode == CommonUtils.Messages.MachineMode.Automatic &&
                this.machineVolatileDataProvider.MachinePowerState == CommonUtils.Messages.MachinePowerState.Powered;

            if (canStart)
            {
                var activeMission = this.missionsDataProvider.GetAllExecutingMissions();
                canStart = !activeMission.Any(x => x.Status == MissionStatus.Executing);
            }

            if (canStart)
            {
                var bay = this.baysDataProvider.GetByNumberShutter(BayNumber.BayOne);
                canStart = bay.Shutter != null && bay.Shutter.Type != ShutterType.NotSpecified || bay.CurrentMission == null;
            }

            return canStart;
        }

        #endregion
    }
}
