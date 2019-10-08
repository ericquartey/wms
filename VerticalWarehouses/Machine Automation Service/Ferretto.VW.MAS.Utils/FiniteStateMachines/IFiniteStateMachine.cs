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

        CommandMessage StartData { get; }

        #endregion

        #region Methods

        void Start(CommandMessage commandMessage, CancellationToken cancellationToken);

        void Stop(StopRequestReason reason);

        #endregion
    }
}
