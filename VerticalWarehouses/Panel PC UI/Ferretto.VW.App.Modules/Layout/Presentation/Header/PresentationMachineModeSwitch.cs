using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachineModeSwitch : BasePresentationViewModel, IDisposable
    {
        #region Fields

        private readonly SubscriptionToken machinePowerChangedToken;

        private bool isDisposed;

        #endregion

        #region Constructors

        public PresentationMachineModeSwitch()
            : base(PresentationTypes.MachineMode)
        {
            this.machinePowerChangedToken = this.EventAggregator
                .GetEvent<PubSubEvent<MachineModeChangedEventArgs>>()
                .Subscribe(
                    this.OnMachineModeChanged,
                    ThreadOption.UIThread,
                    false);
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.machinePowerChangedToken.Dispose();
                }

                this.isDisposed = true;
            }
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
        }

        #endregion
    }
}
