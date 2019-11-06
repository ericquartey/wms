using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Models
{
    internal class ShutterPositioningStateData : IShutterPositioningStateData
    {
        #region Constructors

        public ShutterPositioningStateData(IStateMachine parentMachine, IMachineData machineData)
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
