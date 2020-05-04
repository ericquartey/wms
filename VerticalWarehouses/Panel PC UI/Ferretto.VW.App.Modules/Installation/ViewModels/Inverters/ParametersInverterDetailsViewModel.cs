using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParametersInverterDetailsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private Inverter inverterParameters;

        private bool isBusy;

        private DelegateCommand setInverterParamertersCommand;

        #endregion

        #region Constructors

        public ParametersInverterDetailsViewModel(IMachineDevicesWebService machineDevicesWebService)
            : base(PresentationMode.Installer)
        {
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public Inverter InverterParameters
        {
            get => this.inverterParameters;
            set => this.SetProperty(ref this.inverterParameters, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public ICommand SetInvertersParamertersCommand =>
                   this.setInverterParamertersCommand
               ??
               (this.setInverterParamertersCommand = new DelegateCommand(
                async () => await this.SaveParametersAsync(), this.CanSave));

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.InverterParameters = null;
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            if (this.Data is Inverter inverterParameterSet)
            {
                this.InverterParameters = inverterParameterSet;
            }

            this.IsBackNavigationAllowed = true;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.setInverterParamertersCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.isBusy;
        }

        private async Task SaveParametersAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                await this.machineDevicesWebService.ProgramInverterAsync((byte)this.inverterParameters.Index, new VertimagConfiguration());

                this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        #endregion
    }
}
