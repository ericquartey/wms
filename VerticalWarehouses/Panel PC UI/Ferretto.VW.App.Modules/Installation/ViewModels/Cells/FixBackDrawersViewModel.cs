using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Telemetry.Contracts.Hub;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class FixBackDrawersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IDialogService dialogService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly ISessionService sessionService;

        private readonly ITelemetryHubClient telemetryHubClient;

        private bool isFree;

        private DelegateCommand saveCommand;

        private double stepValue;

        #endregion

        #region Constructors

        public FixBackDrawersViewModel(
            IMachineCellsWebService machineCellsWebService,
            ISessionService sessionService,
            ITelemetryHubClient telemetryHubClient,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(PresentationMode.Installer)
        {
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.telemetryHubClient = telemetryHubClient ?? throw new ArgumentNullException(nameof(telemetryHubClient));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
        }

        #endregion

        #region Properties

        public bool IsFree
        {
            get => this.isFree;
            set => this.SetProperty(ref this.isFree, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveCommand =>
                                            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public double StepValue
        {
            get => this.stepValue;
            set => this.SetProperty(ref this.stepValue, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.StepValue = 0.0;
            this.IsFree = true;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.StepValue = 0.0;
            this.IsFree = true;

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return this.StepValue != 0 && this.IsFree;
        }

        private async Task SaveAsync()
        {
            this.IsFree = false;

            try
            {
                var dialogResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), string.Format(Localized.Get("InstallationApp.FixBackDrawersAsk"), this.StepValue), DialogType.Question, DialogButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    var cells = IEnumConvert(this.MachineService.CellsPlus.Where(s => s.Side == WarehouseSide.Back).OrderBy(s => s.Id));

                    this.Logger.Debug($"Change back drawers position by {this.stepValue}.");

                    await this.TelemetryLoginLogoutAsync($"Change back drawers position by {this.stepValue}.");

                    cells.ForEach(c => c.Position += this.stepValue);

                    await this.machineCellsWebService.SaveCellsAsync(cells);

                    this.NavigationService.GoBack();

                    this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsFree = true;
            }
        }

        private async Task TelemetryLoginLogoutAsync(string action, string supportToken = "")
        {
            var bay = await this.bayManager.GetBayAsync();

            var errorLog = new ErrorLog
            {
                AdditionalText = $"{action} {this.sessionService.UserAccessLevel} {supportToken} {this.sessionService.UserAccessLevel}",
                BayNumber = (int)bay.Number,
                Code = 0,
                DetailCode = (int)this.sessionService.UserAccessLevel,
                ErrorId = int.Parse(DateTime.Now.ToString("-MMddHHmmss")),
                InverterIndex = 0,
                OccurrenceDate = DateTimeOffset.Now,
                ResolutionDate = null
            };

            await this.telemetryHubClient.SendErrorLogAsync(errorLog);
        }

        #endregion
    }
}
