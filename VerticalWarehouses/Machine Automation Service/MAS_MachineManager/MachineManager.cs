using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using System.Collections.Generic;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly IDataLayer dataLayer;

        private readonly IFiniteStateMachines finiteStateMachines;

        private readonly decimal resolution;

        private List<int> cellList;

        #endregion

        #region Constructors

        public MachineManager(IFiniteStateMachines finiteStateMachines, IDataLayer dataLayer)
        {
            this.finiteStateMachines = finiteStateMachines;
            this.dataLayer = dataLayer;
            this.resolution = dataLayer.GetDecimalConfigurationValue(ConfigurationValueEnum.homingCreepSpeed);
            // this.cellList = dataLayer.GetCellList();
        }

        #endregion
    }
}
