using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveStartState : MissionMoveBase, IMissionMoveStartState
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<MachineManagerService> logger;

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly ISensorsProvider sensorsProvider;

        #endregion

        #region Constructors

        public MissionMoveStartState(Mission mission,
            IServiceProvider serviceProvider)
            : base(mission, serviceProvider)
        {
            this.missionsDataProvider = this.ServiceProvider.GetRequiredService<IMissionsDataProvider>();
            this.cellsProvider = this.ServiceProvider.GetRequiredService<ICellsProvider>();
            this.errorsProvider = this.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.sensorsProvider = this.ServiceProvider.GetRequiredService<ISensorsProvider>();
            this.elevatorDataProvider = this.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.machineModeDataProvider = this.ServiceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
            this.loadingUnitsDataProvider = this.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();

            this.logger = this.ServiceProvider.GetRequiredService<ILogger<MachineManagerService>>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command)
        {
            this.Mission.Status = MissionStatus.Executing;
            this.missionsDataProvider.Update(this.Mission);
            this.logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            return true;
        }

        public override void OnNotification(NotificationMessage message)
        {
        }

        #endregion
    }
}
