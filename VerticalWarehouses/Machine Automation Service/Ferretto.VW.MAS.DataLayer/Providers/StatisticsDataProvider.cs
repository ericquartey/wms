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
                return this.dataContext.MachineStatistics.FirstOrDefault(s => s.Id == id);
            }
        }

        public MachineStatistics GetLastConfirmed()
        {
            lock (this.dataContext)
            {
                int dim = this.dataContext.MachineStatistics.Count();

                if (dim > 1)
                {
                    return this.dataContext.MachineStatistics.ElementAt(dim - 1);
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
