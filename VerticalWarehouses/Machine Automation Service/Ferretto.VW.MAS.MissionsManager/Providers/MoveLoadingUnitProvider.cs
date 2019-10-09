using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MissionsManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    internal class MoveLoadingUnitProvider : BaseProvider, IMoveLoadingUnitProvider
    {
        #region Constructors

        protected MoveLoadingUnitProvider(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        public void MoveFromBayToBay(LoadingUnitDestination sourceBay, LoadingUnitDestination destinationBay, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void MoveFromBayToCell(LoadingUnitDestination sourceBay, int destinationCellId, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void MoveFromCellToBay(int sourceCellId, LoadingUnitDestination destinationBay, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void MoveFromCellToCell(int sourceCellId, int destinationCellId, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void MoveLoadingUnitToBay(int loadingUnitId, LoadingUnitDestination destination, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void MoveLoadingUnitToCell(int loadingUnitId, int destinationCellId, BayNumber requestingBay)
        {
            throw new NotImplementedException();
        }

        public void StopMoving()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
