using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.App.Services
{
    internal sealed class BayManager : IBayManager
    {
        #region Fields

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

            eventAggregator
                .GetEvent<PubSubEvent<BayChainPositionChangedEventArgs>>()
                .Subscribe(
                    this.OnBayChainPositionChanged,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Properties

        public double ChainPosition { get; private set; }

        public MachineIdentity Identity { get; private set; }

        public IMachineIdentityWebService IdentityService => this.machineIdentityWebService;

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

        public async Task<Bay> GetBayAsync()
        {
            var bayNumber = ConfigurationManager.AppSettings.GetBayNumber();

            return await this.machineBaysWebService.GetByNumberAsync(bayNumber);
        }

        public async Task InitializeAsync()
        {
            this.Identity = await this.IdentityService.GetAsync();
        }

        private void OnBayChainPositionChanged(BayChainPositionChangedEventArgs e)
        {
            this.ChainPosition = e.Position;
        }

        #endregion
    }
}
