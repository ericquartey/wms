using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private int proceduresCompleted;

        private int proceduresCompletedPercent;

        private int proceduresCount;

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

        public int ProceduresCompleted
        {
            get => this.proceduresCompleted;
            set => this.SetProperty(ref this.proceduresCompleted, value, this.RaiseCanExecuteChanged);
        }

        public int ProceduresCompletedPercent
        {
            get => this.proceduresCompletedPercent;
            set => this.SetProperty(ref this.proceduresCompletedPercent, value, this.RaiseCanExecuteChanged);
        }

        public int ProceduresCount
        {
            get => this.proceduresCount;
            set => this.SetProperty(ref this.proceduresCount, value, this.RaiseCanExecuteChanged);
        }

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
            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.UpdateSetupStatusAsync();
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

        private async Task UpdateSetupStatusAsync()
        {
            var status = await this.machineSetupStatusWebService.GetAsync();

            this.source = new List<dynamic>();
            this.source.Add(new { Text = InstallationApp.VerticalAxisHomedDone, Status = status.VerticalOriginCalibration.IsCompleted ? "CheckCircle" : "CloseCircleOutline" });
            this.source.Add(new { Text = InstallationApp.VerticalResolutionDone, Status = status.VerticalResolutionCalibration.IsCompleted ? "CheckCircle" : "CloseCircleOutline" });
            this.source.Add(new { Text = InstallationApp.VerticalOffsetVerify, Status = status.VerticalOffsetCalibration.IsCompleted ? "CheckCircle" : "CloseCircleOutline" });
            this.source.Add(new
            {
                Text = InstallationApp.BeltBurnishingDone,
                Status = status.BeltBurnishing.InProgress ? "ProgressCheck" : status.BeltBurnishing.IsCompleted ? "CheckCircle" : "CloseCircleOutline",
            });
            this.source.Add(new { Text = InstallationApp.CellsControl, Status = status.CellPanelsCheck.IsCompleted && status.CellsHeightCheck.IsCompleted ? "CheckCircle" : "CloseCircleOutline" });
            this.source.Add(new { Text = InstallationApp.BayHeightCheck, Status = false ? "CheckCircle" : "CloseCircleOutline" });
            this.source.Add(new
            {
                Text = InstallationApp.LoadFirstDrawerPageHeader,
                Status = status.AllLoadingUnits.IsCompleted ? "CheckCircle" : "CloseCircleOutline",
            });

            this.source.Add(new { Text = "Conferma collaudo", Status = status.IsComplete ? "CheckCircle" : "CloseCircleOutline" });

            this.ProceduresCount = this.source.Count;
            this.ProceduresCompleted = this.source.Count(c => c.Status == "CheckCircle");
            this.ProceduresCompletedPercent = (int)((double)this.ProceduresCompleted / (double)this.ProceduresCount * 100.0);

            this.RaisePropertyChanged(nameof(this.Source));
        }

        #endregion
    }
}
