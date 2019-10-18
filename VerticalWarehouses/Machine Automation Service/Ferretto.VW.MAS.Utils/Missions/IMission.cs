using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.Utils.Missions
{
    public interface IMission : IDisposable
    {
        #region Properties

        Guid Id { get; }

        MissionType Type { get; }

        #endregion

        #region Methods

        bool AllowMultipleInstances(CommandMessage command);

        void EndMachine();

        void RemoveHandler(EventHandler<FiniteStateMachinesEventArgs> endHandler);

        void StartMachine(CommandMessage command);

        void StopMachine(StopRequestReason reason);

        #endregion
    }
}
