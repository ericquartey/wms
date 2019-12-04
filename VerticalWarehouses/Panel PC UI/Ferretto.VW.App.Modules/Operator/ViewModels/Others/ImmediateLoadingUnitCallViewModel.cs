using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ImmediateLoadingUnitCallViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly List<LoadingUnit> loadingUnits = new List<LoadingUnit>();

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private int currentItemIndex;

        private DelegateCommand downSelectionCommand;

        private bool isSearching;

        private bool isWaitingForResponse;

        private DelegateCommand loadingUnitCallCommand;

        private int? loadingUnitId;

        private LoadingUnit selectedUnitUnit;

        private DelegateCommand upSelectionCommand;

        #endregion

        #region Constructors

        public ImmediateLoadingUnitCallViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
        }

        #endregion

        #region Properties

        public ICommand DownSelectionCommand =>
            this.downSelectionCommand
            ??
            (this.downSelectionCommand = new DelegateCommand(
                this.SelectNextLoadingUnitAsync,
                this.CanSelectNextItem));

        public bool IsSearching
        {
            get => this.isSearching;
            set => this.SetProperty(ref this.isSearching, value, this.RaiseCanExecuteChanged);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            private set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value) && value)
                {
                    this.ClearNotifications();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public override bool KeepAlive => true;

        public ICommand LoadingUnitCallCommand =>
            this.loadingUnitCallCommand
            ??
            (this.loadingUnitCallCommand = new DelegateCommand(
                async () => await this.RequestLoadingUnitCallAsync(),
                this.CanRequestLoadingUnitCall));

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<LoadingUnit> LoadingUnits => new BindingList<LoadingUnit>(this.loadingUnits);

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedUnitUnit;
            set
            {
                if (this.SetProperty(ref this.selectedUnitUnit, value))
                {
                    this.LoadingUnitId = this.selectedUnitUnit?.Id;
                }
            }
        }

        public ICommand UpSelectionCommand =>
            this.upSelectionCommand
            ??
            (this.upSelectionCommand = new DelegateCommand(
                this.SelectPreviousLoadingUnitAsync,
                this.CanSelectPreviousItem));

        #endregion

        #region Methods

        public async Task GetLoadingUnitsAsync()
        {
            try
            {
                this.loadingUnits.Clear();
                var loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
                this.loadingUnits.AddRange(loadingUnits);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.loadingUnits.Clear();
                this.SelectedLoadingUnit = null;
                this.currentItemIndex = 0;
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.LoadingUnits));
                this.IsSearching = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.currentItemIndex = 0;
            this.LoadingUnitId = null;
      
            await this.GetLoadingUnitsAsync();
            this.SelectLoadingUnit();
        }

        public async Task RequestLoadingUnitCallAsync()
        {
            try
            {
                if (!this.loadingUnitId.HasValue)
                {
                    this.ShowNotification("Id loading unit does not exists.", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.MoveToBayAsync(this.LoadingUnitId.Value);

                this.ShowNotification($"Successfully requested loading unit '{this.SelectedLoadingUnit.Id}'.", Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.LoadingUnitId = null;
                this.IsWaitingForResponse = false;
            }
        }

        public void SelectNextLoadingUnitAsync()
        {
            if (this.currentItemIndex == (this.loadingUnits.Count - 1))
            {
                return;
            }

            this.currentItemIndex++;
            this.SelectLoadingUnit();
        }

        public void SelectPreviousLoadingUnitAsync()
        {
            if (this.currentItemIndex == 0)
            {
                return;
            }

            this.currentItemIndex--;
            this.SelectLoadingUnit();
        }

        private bool CanRequestLoadingUnitCall()
        {
            return
                this.SelectedLoadingUnit != null
                &&
                this.LoadingUnitId.HasValue
                &&
                this.LoadingUnitId > 0
                &&
                !this.IsWaitingForResponse
                &&
                this.loadingUnits.Any(l => l.Id == this.loadingUnitId);
        }

        private bool CanSelectNextItem()
        {
            return
                this.currentItemIndex < this.loadingUnits.Count - 1
                &&
                !this.IsSearching;
        }

        private bool CanSelectPreviousItem()
        {
            return
                this.currentItemIndex > 0
                &&
                !this.IsSearching;
        }

        private void RaiseCanExecuteChanged()
        {
            this.loadingUnitCallCommand?.RaiseCanExecuteChanged();
            this.upSelectionCommand?.RaiseCanExecuteChanged();
            this.downSelectionCommand?.RaiseCanExecuteChanged();
        }

        private void SelectLoadingUnit()
        {
            this.SelectedLoadingUnit = this.loadingUnits.ElementAt(this.currentItemIndex);
            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
