using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using System.Collections.Generic;
using System;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly IDataLayer dataLayer;

        private readonly IFiniteStateMachines finiteStateMachines;

        private List<int> cellList;

        private decimal resolution;

        #endregion

        #region Constructors

        public MachineManager(IFiniteStateMachines finiteStateMachines, IDataLayer dataLayer)
        {
            this.finiteStateMachines = finiteStateMachines;
            this.dataLayer = dataLayer;
            this.dataLayer.SetDecimalConfigurationValue(ConfigurationValueEnum.resolution, 100m);
            this.resolution = dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.resolution);
            // this.cellList = dataLayer.GetCellList();
        }

        #endregion

        #region Methods

        private int GetCellAltitudeInImpulse(int cellID)
        {
            if (!(cellID >= 0 && cellID <= this.cellList.Count))
            {
                throw new ArgumentOutOfRangeException("Given CellID is negative or larger than the number of cells.");
            }
            return Decimal.ToInt32(this.cellList[cellID] * this.resolution); // TODO this.cellList[cellID].Coord
        }

        private void UpdateResolution()
        {
            this.resolution = this.dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.resolution);
        }

        #endregion
    }
}
