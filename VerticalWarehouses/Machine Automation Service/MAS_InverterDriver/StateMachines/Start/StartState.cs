using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.Interface.StateMachines;
using Microsoft.Extensions.Logging;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Start
{
    public class StartState : InverterStateBase
    {
        #region Fields

        private readonly IControlWord controlWord;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public StartState(IInverterStateMachine parentStateMachine, IControlWord controlWord, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogDebug("1:Method Start");

            this.ParentStateMachine = parentStateMachine;
            this.controlWord = controlWord;

            this.controlWord.EnableVoltage = true;
            this.controlWord.QuickStop = true;

            var inverterMessage = new InverterMessage(0x00, (short)InverterParameterId.ControlWordParam, this.parameterValue, SEND_DELAY);

            this.logger.LogTrace($"3:inverterMessage={inverterMessage}");

            parentStateMachine.EnqueueMessage(inverterMessage);

            this.logger.LogDebug("2:Method End");
        }

        #endregion

        #region Methods

        public override bool ProcessMessage(InverterMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override void Stop()
        {
            throw new System.NotImplementedException();
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
