using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitsMissionsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private int currentMissionIndex;

        private DelegateCommand downCommand;

        private IEnumerable<Mission> missions;

        private Mission selectedMission;

        private DelegateCommand upCommand;

        #endregion

        #region Constructors

        public LoadingUnitsMissionsViewModel(
            IMachineMissionsWebService machineMissionsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));

            this.missions = new List<Mission>();
        }

        #endregion

        #region Properties

        public ICommand DownCommand =>
            this.downCommand
            ??
            (this.downCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(false), this.CanDown));

        public override EnableMask EnableMask => EnableMask.Any;

        public IList<Mission> Missions => new List<Mission>(this.missions);

        public Mission SelectedMission
        {
            get => this.selectedMission;
            set => this.SetProperty(ref this.selectedMission, value);
        }

        public ICommand UpCommand =>
            this.upCommand
            ??
            (this.upCommand = new DelegateCommand(() => this.ChangeSelectedListAsync(true), this.CanUp));

        #endregion

        #region Methods

        public void ChangeSelectedListAsync(bool isUp)
        {
            if (this.missions == null)
            {
                return;
            }

            if (this.missions.Any())
            {
                this.currentMissionIndex = isUp ? --this.currentMissionIndex : ++this.currentMissionIndex;
                if (this.currentMissionIndex < 0 || this.currentMissionIndex >= this.missions.Count())
                {
                    this.currentMissionIndex = (this.currentMissionIndex < 0) ? 0 : this.missions.Count() - 1;
                }

                this.SelectListRow();
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            await this.LoadListRowsAsync();
            this.SelectListRow();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.upCommand?.RaiseCanExecuteChanged();
            this.downCommand?.RaiseCanExecuteChanged();
        }

        private bool CanDown()
        {
            return
              this.currentMissionIndex < this.missions.Count() - 1;
        }

        private bool CanUp()
        {
            return
                this.currentMissionIndex > 0;
        }

        private async Task LoadListRowsAsync()
        {
            try
            {
                this.missions = await this.machineMissionsWebService.GetAllAsync();
                this.RaisePropertyChanged(nameof(this.Missions));
                this.SelectedMission = this.missions.FirstOrDefault();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.ToString(), Services.Models.NotificationSeverity.Error);
            }
        }

        private void SelectListRow()
        {
            this.SelectedMission = this.missions.ElementAt(this.currentMissionIndex);
            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
