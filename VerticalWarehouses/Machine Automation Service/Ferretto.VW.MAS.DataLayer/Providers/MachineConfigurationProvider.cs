using System;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public class MachineConfigurationProvider : IMachineConfigurationProvider
    {
        #region Fields

        private const int MaxDrawerGrossWeight = 990;

        private readonly IElevatorDataProvider elevatorDataProvider;

        #endregion

        #region Constructors

        public MachineConfigurationProvider(IElevatorDataProvider elevatorDataProvider)
        {
            if (elevatorDataProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorDataProvider));
            }

            this.elevatorDataProvider = elevatorDataProvider;
        }

        #endregion

        #region Methods

        public bool IsOneKMachine()
        {
            var maximumLoadOnBoard = this.elevatorDataProvider.GetMaximumLoadOnBoard();
            return maximumLoadOnBoard == MaxDrawerGrossWeight;
        }

        #endregion
    }
}
