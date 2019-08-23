using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachineMarch : BasePresentation
    {
        #region Fields

        private readonly IMachineModeService machineModeService;

        private readonly SubscriptionToken subscriptionToken;

        private bool isBusy;

        private bool isMachinePoweredOn;

        #endregion

        #region Constructors

        public PresentationMachineMarch(IMachineModeService machineModeService)
            : base(PresentationTypes.MachineMarch)
        {
            if (machineModeService is null)
            {
                throw new System.ArgumentNullException(nameof(machineModeService));
            }

            this.machineModeService = machineModeService;

            this.subscriptionToken = this.machineModeService.MachineModeChangedEvent
                .Subscribe(
                    this.OnMachineModeChanged,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsMachinePoweredOn
        {
            get => this.isMachinePoweredOn;
            set => this.SetProperty(ref this.isMachinePoweredOn, value);
        }

        #endregion

        #region Methods

        public override async Task ExecuteAsync()
        {
            this.IsBusy = true;

            if (this.IsMachinePoweredOn)
            {
                await this.machineModeService.PowerOffAsync();
            }
            else
            {
                var messageBoxResult = MessageBox.Show("Confirmation operation?", "March", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    await this.machineModeService.PowerOnAsync();
                }
            }

            this.IsBusy = false;
        }

        protected override bool CanExecute()
        {
            return !this.isBusy;
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.IsMachinePoweredOn = e.MachinePower == Services.Models.MachinePowerState.Powered;
        }

        #endregion
    }
}
