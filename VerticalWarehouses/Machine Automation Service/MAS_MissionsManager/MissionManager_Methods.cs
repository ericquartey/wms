using System.Threading.Tasks;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;
using System.Collections.Generic;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_MissionsManager
{
    public partial class MissionsManager
    {
        #region Methods

        private void ChooseAndExecuteMission()
        {
        }

        private void DefineBay(INewConnectedClientMessageData data)
        {
            // TODO to be implemented
            this.baysManager.Bays[0].IsConnected = true;
            this.baysManager.Bays[0].Status = BayStatus.Available;
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

        private void ExecuteMission(Mission mission)
        {
        }

        private async Task GetMissions()
        {
            try
            {
                var machineId = 1; // TODO get machine's Id from GeneralInfo
                var missions = await this.machinesDataService.GetMissionsByIdAsync(machineId);
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
            return true;
        }

        private bool IsAnyMissionExecutable()
        {
            return true;
        }

        #endregion
    }
}
