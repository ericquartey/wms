using System.Threading.Tasks;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using IIdentityService = Ferretto.VW.MAS.AutomationService.Contracts.IIdentityService;

namespace Ferretto.VW.App.Services
{
    public class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly IIdentityService identityService;

        #endregion

        #region Constructors

        public MachineProvider(IIdentityService identityService)
        {
            if (identityService == null)
            {
                throw new System.ArgumentNullException(nameof(identityService));
            }

            this.identityService = identityService;
        }

        #endregion

        #region Methods

        public async Task<MachineIdentity> GetIdentityAsync()
        {
            var machineIdentity = await this.identityService.GetAsync();

            return new MachineIdentity
            {
                AreaId = machineIdentity.AreaId,
                BayId = machineIdentity.BayId,
                Id = machineIdentity.Id,
                InstallationDate = machineIdentity.InstallationDate,
                LastServiceDate = machineIdentity.LastServiceDate,
                ModelName = machineIdentity.ModelName,
                NextServiceDate = machineIdentity.NextServiceDate,
                SerialNumber = machineIdentity.SerialNumber,
                ServiceStatus = (MachineServiceStatus)machineIdentity.ServiceStatus,
                TrayCount = machineIdentity.TrayCount,
            };
        }

        #endregion
    }
}
