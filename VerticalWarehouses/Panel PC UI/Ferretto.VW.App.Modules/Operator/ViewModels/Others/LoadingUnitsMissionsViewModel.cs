using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitsMissionsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private const int PollIntervalMilliseconds = 5000;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly ISessionService sessionService;

        private int currentMissionIndex;

        private DelegateCommand deleteMissionCommand;

        private bool isDeleteMissionCommand;

        private ObservableCollection<Mission> missions = new ObservableCollection<Mission>();

        private Mission selectedMission;

        #endregion

        #region Constructors

        public LoadingUnitsMissionsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            ISessionService sessionService,
            IMachineMissionsWebService machineMissionsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));

            this.Missions = new ObservableCollection<Mission>();
        }

        #endregion

        #region Properties

        public System.Windows.Input.ICommand DeleteMissionCommand =>
            this.deleteMissionCommand
            ??
            (this.deleteMissionCommand = new DelegateCommand(
                async () => await this.DeleteMissionAsync(),
                this.CanDeleteMission));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsDeleteMissionCommand
        {
            get => this.isDeleteMissionCommand;
            set => this.SetProperty(ref this.isDeleteMissionCommand, value, this.RaiseCanExecuteChanged);
        }

        public ObservableCollection<Mission> Missions
        {
            get => this.missions;
            set => this.SetProperty(ref this.missions, value, this.RaiseCanExecuteChanged);
        }

        public Mission SelectedMission
        {
            get => this.selectedMission;
            set => this.SetProperty(ref this.selectedMission, value);
        }

        #endregion

        #region Methods

        public void ChangeSelectedList(bool isUp)
        {
            if (this.missions is null)
            {
                return;
            }

            if (this.missions.Any())
            {
                var newIndex = isUp ? this.currentMissionIndex - 1 : this.currentMissionIndex + 1;

                this.currentMissionIndex = Math.Max(0, Math.Min(newIndex, this.missions.Count - 1));
            }

            this.SelectLoadingUnit();
        }

        public ObservableCollection<T> Convert<T>(IEnumerable<T> original)
        {
            return new ObservableCollection<T>(original);
        }

        public async Task DeleteMissionAsync()
        {
            try
            {
                this.machineLoadingUnitsWebService.AbortAsync(this.selectedMission.Id, this.SelectedMission.TargetBay);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            this.missions.Clear();
            this.RaisePropertyChanged(nameof(this.Missions));
        }

        public override async Task OnAppearedAsync()
        {
            this.IsDeleteMissionCommand = this.sessionService.UserAccessLevel == UserAccessLevel.Operator;

            this.RaisePropertyChanged(nameof(this.IsDeleteMissionCommand));

            await base.OnAppearedAsync();

            await this.RefreshMissionsAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.LoadListRowsAsync();
        }

        private bool CanDeleteMission()
        {
            return this.selectedMission != null
                && (this.MachineModeService.MachineMode == MachineMode.Manual || this.MachineModeService.MachineMode == MachineMode.Manual2 || this.MachineModeService.MachineMode == MachineMode.Manual3);
        }

        private async Task LoadListRowsAsync()
        {
            try
            {
                var lastMissionId = this.selectedMission?.Id;
                var newMissions = await this.machineMissionsWebService.GetAllAsync();

                this.missions.Clear();
                this.Missions = this.Convert(newMissions);

                this.RaisePropertyChanged(nameof(this.Missions));

                this.SetCurrentIndex(lastMissionId);

                this.SelectLoadingUnit();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.ToString(), Services.Models.NotificationSeverity.Error);
            }
        }

        private async Task RefreshMissionsAsync()
        {
            while (this.IsVisible)
            {
                await Task.Delay(PollIntervalMilliseconds);
                await this.LoadListRowsAsync();
            }
        }

        private void SelectLoadingUnit()
        {
            if (this.missions.Any())
            {
                this.SelectedMission = this.missions.ElementAt(this.currentMissionIndex);
            }
            else
            {
                this.SelectedMission = null;
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetCurrentIndex(int? missionId)
        {
            if (missionId.HasValue
                &&
                this.missions.FirstOrDefault(l => l.Id == missionId.Value) is Mission missionFound)
            {
                this.currentMissionIndex = this.missions.IndexOf(missionFound);
            }
            else
            {
                this.currentMissionIndex = 0;
            }
        }

        #endregion
    }
}
