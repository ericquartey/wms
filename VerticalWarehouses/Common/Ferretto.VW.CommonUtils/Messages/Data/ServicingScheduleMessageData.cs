using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ServicingScheduleMessageData : IMessageData
    {
        #region Constructors

        public ServicingScheduleMessageData()
        {
        }

        public ServicingScheduleMessageData(int serviceId, MachineServiceStatus serviceStatus, int instructionId, MachineServiceStatus instructionStatus, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ServiceId = serviceId;
            this.ServiceStatus = serviceStatus;
            this.InstructionStatus = instructionStatus;
            this.InstructionId = instructionId;
        }

        #endregion

        #region Properties

        public int InstructionId { get; set; }

        public MachineServiceStatus InstructionStatus { get; set; }

        public int ServiceId { get; set; }

        public MachineServiceStatus ServiceStatus { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
