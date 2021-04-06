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
    internal sealed class BayManager : IBayManager, IDisposable
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private SubscriptionToken chainPositionChangedToken;

        private SubscriptionToken healthStatusChangedToken;

        private bool isDisposed;

        #endregion

        #region Constructors

        public BayManager(
            IEventAggregator eventAggregator,
            IMachineAccessoriesWebService accessoriesWebService,
            IMachineBaysWebService machineBaysWebService,
            IMachineIdentityWebService machineIdentityWebService)
        {
            this.eventAggregator = eventAggregator;
            this.accessoriesWebService = accessoriesWebService;
            this.machineBaysWebService = machineBaysWebService;
            this.machineIdentityWebService = machineIdentityWebService;

            this.chainPositionChangedToken = this.chainPositionChangedToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                    .Subscribe(
                        this.OnBayChainPositionChanged,
                        ThreadOption.UIThread,
                        false);

            this.healthStatusChangedToken = this.healthStatusChangedToken
                ??
                this.eventAggregator
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

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.chainPositionChangedToken?.Dispose();
                this.chainPositionChangedToken = null;

                this.healthStatusChangedToken?.Dispose();
                this.healthStatusChangedToken = null;

                this.isDisposed = true;
            }
        }

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
            return await this.accessoriesWebService.GetAllAsync();
        }

        public async Task<Bay> GetBayAsync()
        {
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            return await this.machineBaysWebService.GetByNumberAsync(bayNumber);
        }

        public async Task InitializeAsync()
        {
            if (this.Identity != null
                && this.Identity.AreaId != null
                )
            {
                return;
            }

            try
            {
                this.Identity = await this.machineIdentityWebService.GetAsync();
            }
            catch
            {
                // do nothing
            }
        }

        public async Task SetAlphaNumericBarAsync(bool isEnabled, IPAddress ipAddress, int port, AlphaNumericBarSize size)
        {
            await this.accessoriesWebService.UpdateAlphaNumericBarAsync(isEnabled, ipAddress.ToString(), port, size);
        }

        public async Task SetLaserPointerAsync(bool isEnabled, IPAddress ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition)
        {
            await this.accessoriesWebService.UpdateLaserPointerAsync(isEnabled, ipAddress.ToString(), port, xOffset, yOffset, zOffsetLowerPosition, zOffsetUpperPosition);
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.ChainPosition = e.Position;
        }

        private async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            if (e.HealthMasStatus == HealthStatus.Degraded || e.HealthMasStatus == HealthStatus.Healthy)
            {
                await this.InitializeAsync();
            }
        }

        #endregion
    }
}
