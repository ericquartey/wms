using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.FiniteStateMachines
{
    public interface IFiniteStateMachine
    {
        #region Events

        event EventHandler<FiniteStateMachinesEventArgs> Completed;

        #endregion

        #region Properties

        Guid InstanceId { get; }

        #endregion

        #region Methods

        bool AllowMultipleInstances(CommandMessage command);

        void Start(CommandMessage commandMessage, IFiniteStateMachineData machineData, CancellationToken cancellationToken);

        void Stop(StopRequestReason reason);

        #endregion
    }
}
