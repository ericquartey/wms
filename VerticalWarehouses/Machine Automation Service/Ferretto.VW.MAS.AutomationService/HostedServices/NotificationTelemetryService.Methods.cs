using System;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    public partial class NotificationTelemetryService
    {
        #region Fields

        private string serialNumber;

        #endregion

        #region Methods

        private async Task OnDataLayerReady(IServiceProvider serviceProvider)
        {
            this.Logger.LogTrace("OnDataLayerReady start");
            var baysDataProvider = serviceProvider.GetRequiredService<IMachineProvider>();
            var bays = baysDataProvider.Get();
            this.serialNumber = bays.SerialNumber;

            this.Logger.LogTrace("OnDataLayerReady end");
        }

        private async Task OnMoveLoadingUnit(MoveLoadingUnitMessageData message)
        {
            var missionLog = new MissionLog()
            {
                //Destination = message.Destination.ToString(),
                CellId = message.DestinationCellId,
                //EjectLoadUnit = message.,
                //Id = message.,
                LoadUnitId = message.LoadUnitId.Value,
                MissionId = message.MissionId.Value,
                MissionType = message.MissionType.ToString(),
                //Bay = message,
                //Direction = message.,
                //CreationDate = message.,
                //Priority = message.,
                //Status = message.,
                Step = (int)message.MissionStep,
                StopReason = (int)message.StopReason,
                TimeStamp = DateTime.Now,
                WmsId = message.WmsId
            };

            this.Logger.LogTrace("OnDataLayerReady end");
            await this.telemetryHub.SendMissionLog(this.serialNumber, missionLog);
        }

        #endregion
    }
}
