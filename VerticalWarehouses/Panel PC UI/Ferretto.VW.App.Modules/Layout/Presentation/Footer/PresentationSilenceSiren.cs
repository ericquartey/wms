using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationSilenceSiren : BasePresentationViewModel
    {
        #region Fields

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineIdentityWebService machineIdentity;

        #endregion

        #region Constructors

        public PresentationSilenceSiren(IMachineIdentityWebService machineIdentity)
            : base(PresentationTypes.SilenceSiren)
        {
            this.machineIdentity = machineIdentity ?? throw new ArgumentNullException(nameof(machineIdentity));

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.logger.Debug($"Silences Siren Alarm");
            this.machineIdentity.SilenceSirenAlarmAsync();

            this.IsEnabled = false;
            this.RaiseCanExecuteChanged();

            return Task.CompletedTask;
        }
        protected override bool CanExecute()
        {
            if (!this.IsEnabled.HasValue)
            {
                return false;
            }

            return this.IsEnabled.Value;
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.States != null
                &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation SilenceSiren
                &&
                SilenceSiren.IsVisible.HasValue)
            {
                this.IsVisible = SilenceSiren.IsVisible;
                this.IsEnabled = SilenceSiren.IsEnabled;
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
