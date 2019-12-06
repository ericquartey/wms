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

        IFiniteStateMachineData MachineData { get; set; }

        #endregion

        #region Methods

        void Abort();

        bool AllowMultipleInstances(CommandMessage command);

        void Pause();

        void Resume(CommandMessage commandMessage, IServiceProvider serviceProvider, CancellationToken cancellationToken);

        void Start(CommandMessage commandMessage, IServiceProvider serviceProvider, CancellationToken cancellationToken);

        void Stop(StopRequestReason reason);

        #endregion
    }
}
