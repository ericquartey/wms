using System.Threading.Tasks;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;
using System.Collections.Generic;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using System.Linq;

namespace Ferretto.VW.MAS_MissionsManager
{
    public partial class MissionsManager
    {
        #region Methods

        private void ChooseAndExecuteMission()
        {
            for (int i = 0; i < this.baysManager.Bays.Count; i++)
            {
                if (this.baysManager.Bays[i].IsConnected == true && this.baysManager.Bays[i].Status == BayStatus.Available && this.baysManager.Bays[i].Missions != null && this.baysManager.Bays[i].Missions.Count > 0)
                {
                    var missionsQuantity = this.baysManager.Bays[i].Missions.Count;
                    this.baysManager.Bays[i].Missions.Dequeue(out var mission);
                    this.missionsDataService.ExecuteAsync(mission.Id);
                    var data = new ExecuteMissionMessageData(mission, missionsQuantity);
                    var notificationMessage = new NotificationMessage(data, "Execute Mission", MessageActor.AutomationService, MessageActor.MissionsManager, MessageType.ExecuteMission, MessageStatus.NoStatus);
                    this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                }
            }
        }

        private void DefineBay(INewConnectedClientMessageData data)
        {
            // TODO to be implemented
            this.baysManager.Bays[0].IsConnected = true;
            this.baysManager.Bays[0].Status = BayStatus.Available;
            this.baysManager.Bays[0].Id = 2;
        }

        private void DistributeMissionsToConnectedBays()
        {
            for (int i = 0; i < this.machineMissions.Count; i++)
            {
                var bayId = (int)this.machineMissions[i].BayId;
                for (int j = 0; j < this.baysManager.Bays.Count; j++)
                {
                    if (this.baysManager.Bays[j].Id == bayId)
                    {
                        this.baysManager.Bays[j].Missions.Enqueue(this.machineMissions[i]);
                        this.machineMissions.RemoveAt(i);
                    }
                }
            }
        }

        private async Task GetMissions()
        {
            try
            {
                var machineId = 1; // TODO get machine's Id from GeneralInfo
                var missionsCollection = await this.machinesDataService.GetMissionsByIdAsync(machineId);
                var missions = missionsCollection.Where(x => x.Status == MissionStatus.Executing || x.Status == MissionStatus.New).ToList();
                this.machineMissions = new List<Mission>();
                for (int i = 0; i < missions.Count; i++)
                {
                    this.machineMissions.Add(missions[i]);
                }
            }
            catch (SwaggerException ex)
            {
            }
        }

        private async Task InitializeBays()
        {
            this.baysManager.Bays = new List<MAS_Utils.Utilities.Bay>();
            var baysQuantity = await this.generalInfo.BaysQuantity;
            for (int i = 0; i < baysQuantity; i++)
            {
                this.baysManager.Bays.Add(new MAS_Utils.Utilities.Bay
                {
                    IsConnected = false,
                    Status = BayStatus.Unavailable,
                });
            }
        }

        private bool IsAnyBayServiceable()
        {
            var returnValue = false;
            for (int i = 0; i < this.baysManager.Bays.Count; i++)
            {
                if (this.baysManager.Bays[i].IsConnected == true && this.baysManager.Bays[i].Status == BayStatus.Available)
                {
                    returnValue = true;
                }
            }
            return returnValue;
        }

        private bool IsAnyMissionExecutable()
        {
            var returnValue = false;
            for (int i = 0; i < this.baysManager.Bays.Count; i++)
            {
                if (this.baysManager.Bays[i].IsConnected == true && this.baysManager.Bays[i].Status == BayStatus.Available && this.baysManager.Bays[i].Missions != null && this.baysManager.Bays[i].Missions.Count > 0)
                {
                    returnValue = true;
                }
            }
            return returnValue;
        }

        #endregion
    }
}
