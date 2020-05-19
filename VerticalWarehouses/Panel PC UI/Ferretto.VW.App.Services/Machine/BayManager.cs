using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Services
{
    internal sealed class BayManager : IBayManager
    {
        #region Fields

        private readonly SubscriptionToken chainPositionChangedToken;

        private readonly object healthStatusChangedToken;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        #endregion

        #region Constructors

        public BayManager(
            IEventAggregator eventAggregator,
            IMachineBaysWebService machineBaysWebService,
            IMachineIdentityWebService machineIdentityWebService)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));

            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            this.chainPositionChangedToken = eventAggregator
                .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                .Subscribe(
                    this.OnBayChainPositionChanged,
                    ThreadOption.UIThread,
                    false);

            this.healthStatusChangedToken = eventAggregator
                .GetEvent<PubSubEvent<HealthStatusChangedEventArgs>>()
                .Subscribe(
                    async (e) => await this.OnHealthStatusChangedAsync(e),
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Properties

        public double ChainPosition { get; private set; }

        public MachineIdentity Identity { get; private set; }

        #endregion

        #region Methods

        public async Task<LoadingUnit> GetAccessibleLoadingUnitAsync()
        {
            var bay = await this.GetBayAsync();

            return bay.Positions
                .Where(p => p.LoadingUnit != null)
                .OrderByDescending(p => p.Height)
                .Select(p => p.LoadingUnit)
                .FirstOrDefault();
        }

        public async Task<BayAccessories> GetBayAccessoriesAsync()
        {
            return await this.machineBaysWebService.GetAccessoriesAsync();
        }

        public async Task<Bay> GetBayAsync()
        {
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            return await this.machineBaysWebService.GetByNumberAsync(bayNumber);
        }

        public async Task InitializeAsync()
        {
            try
            {
                this.Identity = await this.machineIdentityWebService.GetAsync();
            }
            catch
            {
                // do nothing
            }
        }

        public async Task SetAlphaNumericBarAsync(bool isEnabled, IPAddress ipAddress, int port)
        {
            await this.machineBaysWebService.SetAlphaNumericBarAsync(isEnabled, ipAddress.ToString(), port);
        }

        public async Task SetSetLaserPointerAsync(bool isEnabled, IPAddress ipAddress, int port, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition)
        {
            await this.machineBaysWebService.SetLaserPointerAsync(isEnabled, ipAddress.ToString(), port, yOffset, zOffsetLowerPosition, zOffsetUpperPosition);
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.ChainPosition = e.Position;
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            await this.InitializeAsync();
        }

        #endregion
    }
}
