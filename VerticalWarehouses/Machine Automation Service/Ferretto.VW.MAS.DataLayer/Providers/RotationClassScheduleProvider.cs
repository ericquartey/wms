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
    internal sealed class RotationClassScheduleProvider : BaseProvider, IRotationClassScheduleProvider
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        private readonly DataLayerContext dataContext;

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<DataLayerService> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public RotationClassScheduleProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            ICellsProvider cellsProvider,
            IMachineProvider machineProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ILogger<DataLayerService> logger) : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void AddOrModifyRotationClassSchedule(RotationClassSchedule rotationClassSchedule)
        {
            lock (this.dataContext)
            {
                if (!this.dataContext.RotationClassSchedule.Any(s => s.Id == rotationClassSchedule.Id))
                {
                    this.AddRotationClassSchedule(rotationClassSchedule);
                }
                else
                {
                    this.ModifyRotationClassSchedule(rotationClassSchedule);
                }
            }
        }

        public void AddRotationClassSchedule(RotationClassSchedule rotationClassSchedule)
        {
            lock (this.dataContext)
            {
                this.dataContext.RotationClassSchedule.Add(rotationClassSchedule);
                this.dataContext.SaveChanges();
            }
        }

        public bool CheckRotationClass()
        {
            lock (this.dataContext)
            {
                var result = this.dataContext.RotationClassSchedule.Any();
                if (!result)
                {
                    var rotationClassSchedule = new RotationClassSchedule()
                    {
                        DaysCount = 7
                    };
                    this.dataContext.RotationClassSchedule.Add(rotationClassSchedule);
                    this.dataContext.SaveChanges();
                }
                return result;
            }
        }

        public IEnumerable<RotationClassSchedule> GetAllRotationClassSchedule()
        {
            lock (this.dataContext)
            {
                var result = this.dataContext.RotationClassSchedule;
                return result;
            }
        }

        public void ModifyRotationClassSchedule(RotationClassSchedule newRotationClassSchedule)
        {
            lock (this.dataContext)
            {
                if (newRotationClassSchedule != null)
                {
                    var rotationClassSchedule = this.dataContext.RotationClassSchedule.Single(s => s.Id == newRotationClassSchedule.Id);
                    rotationClassSchedule.DaysCount = newRotationClassSchedule.DaysCount;
                    rotationClassSchedule.LastSchedule = newRotationClassSchedule.LastSchedule;
                    this.dataContext.RotationClassSchedule.Update(rotationClassSchedule);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void SetRotationClass()
        {
            try
            {
                var machine = this.machineProvider.GetMinMaxHeight();
                if (machine.IsRotationClass)
                {
                    var activeSettings = this.GetAllRotationClassSchedule().FirstOrDefault(x => x.DaysCount > 0 &&
                        (x.LastSchedule == null || DateTime.Now.Subtract(x.LastSchedule.Value).Days >= x.DaysCount));

                    if (activeSettings != null)
                    {
                        if (this.loadingUnitsDataProvider.SetRotationClass())
                        {
                            activeSettings.LastSchedule = DateTime.Now;
                            this.ModifyRotationClassSchedule(activeSettings);
                            this.logger.LogInformation($"SetRotationClass : OK");
                        }
                    }
                    this.cellsProvider.SetRotationClass();
                }
                this.machineVolatileDataProvider.IsOptimizeRotationClass = machine.IsRotationClass;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"SetRotationClass : {ex.Message}");
            }
        }

        #endregion
    }
}
