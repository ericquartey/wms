using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    public enum InstallationStatus
    {
        Incomplete,

        Complete,

        Inprogress,
    }

    [Warning(WarningsArea.Installation)]
    internal sealed class InstallationMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineSetupStatusWebService machineSetupStatusWebService;

        private List<dynamic> source = new List<dynamic>();

        #endregion

        #region Constructors

        public InstallationMenuViewModel(IMachineSetupStatusWebService machineSetupStatusWebService)
            : base()
        {
            this.machineSetupStatusWebService = machineSetupStatusWebService;
        }

        #endregion

        #region Properties

        public List<dynamic> Source
        {
            get
            {
                return this.source;
            }
        }

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            this.source = new List<dynamic>();
            this.source.Add(new { Text = InstallationApp.VerticalAxisHomedDone, Status = "CheckCircle" });
            this.source.Add(new { Text = InstallationApp.VerticalResolutionDone, Status = "CheckCircle" });
            this.source.Add(new { Text = InstallationApp.VerticalOffsetVerify, Status = "CheckCircle" });
            this.source.Add(new { Text = InstallationApp.BeltBurnishingDone, Status = "ProgressCheck" });
            this.source.Add(new { Text = InstallationApp.CellsControl, Status = "CloseCircleOutline" });
            this.source.Add(new { Text = InstallationApp.BayHeightCheck, Status = "CloseCircleOutline" });
            this.source.Add(new { Text = InstallationApp.LoadFirstDrawerPageHeader, Status = "CloseCircleOutline" });
            this.source.Add(new { Text = "Conferma collaudo", Status = "CloseCircleOutline" });


            this.RaiseCanExecuteChanged();

            await base.OnAppearedAsync();
        }

        protected override async Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            await base.OnHealthStatusChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Source));
        }

        #endregion
    }
}
