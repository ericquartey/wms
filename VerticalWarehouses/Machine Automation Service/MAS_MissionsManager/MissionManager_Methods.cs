using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_MissionsManager
{
    public partial class MissionsManager
    {
        #region Methods

        private async Task ChooseAndExecuteMission()
        {
            for (var i = 0; i < this.baysManager.Bays.Count; i++)
            {
                if (this.baysManager.Bays[i].IsConnected == true && this.baysManager.Bays[i].Status == BayStatus.Available && this.baysManager.Bays[i].Missions != null && this.baysManager.Bays[i].Missions.Count > 0)
                {
                    try
                    {
                        await Task.Delay(5000);
                        Mission mission;
                        var missionsQuantity = this.baysManager.Bays[i].Missions.Count - 1;
                        var executingMissions = this.baysManager.Bays[i].Missions.Where(x => x.Status == MissionStatus.Executing).ToList();
                        if (executingMissions.Count == 0)
                        {
                            mission = this.baysManager.Bays[i].Missions.Dequeue();
                            await this.missionsDataService.ExecuteAsync(mission.Id);
                        }
                        else
                        {
                            mission = executingMissions.First();
                        }
                        var data = new ExecuteMissionMessageData(mission, missionsQuantity, this.baysManager.Bays[i].ConnectionId);
                        var notificationMessage = new NotificationMessage(data, "Execute Mission", MessageActor.AutomationService, MessageActor.MissionsManager, MessageType.ExecuteMission, MessageStatus.NoStatus);
                        this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                        this.baysManager.Bays[i].Status = BayStatus.Unavailable;
                        this.logger.LogDebug($"MM MissionManagementCycle: Iteration #{this.logCounterMissionManagement++}: Bay {this.baysManager.Bays[i].Id} status set to Unavailable, chosed mission {mission.Id}");
                        i = this.baysManager.Bays.Count;
                        this.logger.LogDebug($"MM MissionManagementCycle: End iteration #{this.logCounterMissionManagement++}: executing mission {mission.Id}");
                    }
                    catch (SwaggerException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (ArgumentNullException)
                    {
                    }
                }
            }
        }

        private async Task DistributeMissions()
        {
            // TODO get machine Id from DataLayer
            try
            {
                var machineId = 1;
                var missions = await this.machinesDataService.GetMissionsByIdAsync(machineId);
                for (var i = 0; i < this.baysManager.Bays.Count; i++)
                {
                    var bayMissions = missions.Where(x => x.BayId == this.baysManager.Bays[i].Id && x.Status != MissionStatus.Completed).ToList();
                    bayMissions.OrderBy(x => x.Priority);
                    this.baysManager.Bays[i].Missions = new Queue<Mission>();
                    for (var j = 0; j < bayMissions.Count; j++)
                    {
                        this.baysManager.Bays[i].Missions.Enqueue(bayMissions[j]);
                    }
                }
            }
            catch (SwaggerException swaggerException)
            {
                throw new ApplicationException($"MM DistributeMission: {swaggerException.Message}");
            }
            catch (ArgumentNullException argumentNullException)
            {
                throw new ApplicationException($"MM DistributeMission: {argumentNullException.Message}");
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"MM DistributeMission: {exception.Message}");
            }
        }

        private async Task InitializeBays()
        {
            this.baysManager.Bays = new List<MAS_Utils.Utilities.Bay>();
            var ip1 = await this.setupNetwork.PPC1MasterIPAddress;
            var ip2 = await this.setupNetwork.PPC2SlaveIPAddress;
            var ip3 = await this.setupNetwork.PPC3SlaveIPAddress;
            var ipAddresses = new string[] { ip1.ToString(), ip2.ToString(), ip3.ToString() };
            var bayTypes = new int[] { await this.generalInfo.Bay1Type, await this.generalInfo.Bay2Type, await this.generalInfo.Bay3Type };
            var baysQuantity = await this.generalInfo.BaysQuantity;
            for (var i = 0; i < baysQuantity; i++)
            {
                this.baysManager.Bays.Add(new MAS_Utils.Utilities.Bay
                {
                    Id = i == 0 ? 2 : 3,
                    IsConnected = false,
                    Status = BayStatus.Unavailable,
                    IpAddress = ipAddresses[i],
                    Type = (BayTypes)bayTypes[i]
                });
            }
        }

        private bool IsAnyBayServiceable()
        {
            var returnValue = false;
            for (var i = 0; i < this.baysManager.Bays.Count; i++)
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
            for (var i = 0; i < this.baysManager.Bays.Count; i++)
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
