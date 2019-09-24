using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class ElevatorDataProvider : IElevatorDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        protected ElevatorDataProvider(DataLayerContext dataContext, ISetupStatusProvider setupStatusProvider)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.dataContext = dataContext;
            this.setupStatusProvider = setupStatusProvider;
        }

        #endregion

        #region Methods

        public decimal GetMaximumLoadOnBoard()
        {
            var elevator = this.dataContext.Elevators.Single();

            return elevator.StructuralProperties.MaximumLoadOnBoard;
        }

        public void UpdateVerticalResolution(decimal newResolution)
        {
            if (newResolution <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newResolution));
            }

            var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == DataModels.Orientation.Vertical);

            verticalAxis.Resolution = newResolution;
            this.dataContext.SaveChanges();

            this.setupStatusProvider.CompleteVerticalResolution();
        }

        #endregion
    }
}
