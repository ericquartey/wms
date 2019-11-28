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

        Guid FsmId { get; }

        IFiniteStateMachineData MachineData { get; set; }

        FsmType Type { get; }

        #endregion

        #region Methods

        void AbortMachineMission();

        bool AllowMultipleInstances(CommandMessage command);

        void EndMachine();

        void PauseMachineMission();

        void RemoveHandler(EventHandler<FiniteStateMachinesEventArgs> endHandler);

        void ResumeMachineMission();

        void StartMachine(CommandMessage command);

        void StopMachine(StopRequestReason reason);

        #endregion
    }
}
