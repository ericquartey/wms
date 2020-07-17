﻿using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements.Models
{
    internal class CombinedMovementsStateData : ICombinedMovementsStateData
    {
        #region Constructors

        public CombinedMovementsStateData(IStateMachine parentMachine, IMachineData machineData)
        {
            this.ParentMachine = parentMachine;
            this.MachineData = machineData;

            this.StopRequestReason = StopRequestReason.NoReason;
        }

        #endregion

        #region Properties

        public FieldNotificationMessage FieldMessage { get; set; }

        public IMachineData MachineData { get; }

        public IStateMachine ParentMachine { get; }

        public StopRequestReason StopRequestReason { get; set; }

        #endregion
    }
}
