using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.LaserDriver.StateMachines.SwitchOn
{
    internal sealed class SwitchOnStateMachine : LaserStateMachineBase
    {
        #region Constructors

        public SwitchOnStateMachine(
            BayNumber bayNumber,
            IEventAggregator eventAggregator,
            ILogger logger,
            BlockingConcurrentQueue<FieldCommandMessage> laserCommandQueue)
            : base(bayNumber, eventAggregator, logger, laserCommandQueue)
        {
            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.ChangeState(new SwitchOnStartState(this, this.Logger));
        }

        #endregion
    }
}
