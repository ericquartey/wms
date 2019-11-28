using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.VW.MAS.Utils.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Missions
{
    public class MachineMission<TMachine> : Mission<TMachine>
        where TMachine : class, IFiniteStateMachine
    {
        #region Constructors

        public MachineMission(IServiceScopeFactory serviceScopeFactory, EventHandler<FiniteStateMachinesEventArgs> endHandler)
            : base(serviceScopeFactory)
        {
            this.CurrentStateMachine.Completed += endHandler;

            switch (typeof(TMachine).Name)
            {
                case "IChangeRunningStateStateMachine":
                    this.Type = FsmType.ChangeRunningType;
                    break;

                case "IMoveLoadingUnitStateMachine":
                    this.Type = FsmType.MoveLoadingUnit;
                    break;

                default:
                    throw new ArgumentException(typeof(TMachine).Name);
            }

            this.Status = MissionStatus.New;
        }

        #endregion

        #region Properties

        public TMachine MissionMachine => this.CurrentStateMachine;

        #endregion

        #region Methods

        public override bool AllowMultipleInstances(CommandMessage command)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            var returnValue = true;
            switch (command.Type)
            {
                case MessageType.ChangeRunningState:
                    returnValue = false;
                    break;

                case MessageType.MoveLoadingUnit:
                    returnValue = true;
                    break;
            }

            return returnValue;
        }

        #endregion
    }
}
