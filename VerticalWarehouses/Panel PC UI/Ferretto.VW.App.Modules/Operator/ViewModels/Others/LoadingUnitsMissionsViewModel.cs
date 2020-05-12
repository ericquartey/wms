using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitsMissionsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private const int PollIntervalMilliseconds = 5000;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IList<Mission> missions = new List<Mission>();

        private int currentMissionIndex;

        private Mission selectedMission;

        #endregion

        #region Constructors

        public LoadingUnitsMissionsViewModel(
            IMachineMissionsWebService machineMissionsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public IList<Mission> Missions => new List<Mission>(this.missions);

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

        public override void Disappear()
        {
            base.Disappear();

            this.missions.Clear();
            this.RaisePropertyChanged(nameof(this.Missions));
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.RefreshMissionsAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.LoadListRowsAsync();
        }

        private async Task LoadListRowsAsync()
        {
            try
            {
                var lastMissionId = this.selectedMission?.Id;
                var newMissions = await this.machineMissionsWebService.GetAllAsync();

                this.missions.Clear();
                newMissions.ForEach(l => this.missions.Add(l));

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
