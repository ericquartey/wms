﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ImmediateLoadingUnitCallViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly List<LoadingUnit> loadingUnits = new List<LoadingUnit>();

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineService machineService;

        private DelegateCommand callLoadingUnitCommand;

        private int currentItemIndex;

        private DelegateCommand downSelectionCommand;

        private bool isSearching;

        private int? loadingUnitId;

        private DelegateCommand loadingUnitsMissionsCommand;

        private LoadingUnit selectedUnitUnit;

        private DelegateCommand upSelectionCommand;

        #endregion

        #region Constructors

        public ImmediateLoadingUnitCallViewModel(
            IMachineService machineService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
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

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
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
            this.callLoadingUnitCommand
            ??
            (this.callLoadingUnitCommand = new DelegateCommand(
                async () => await this.CallLoadingUnitAsync(),
                this.CanCallLoadingUnit));

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set
            {
                if (this.SetProperty(ref this.loadingUnitId, value))
                {
                    this.CheckToSelectLoadingUnit();
                }
            }
        }

        public IEnumerable<LoadingUnit> LoadingUnits => new BindingList<LoadingUnit>(this.loadingUnits);

        public ICommand LoadingUnitsMissionsCommand =>
            this.loadingUnitsMissionsCommand
            ??
            (this.loadingUnitsMissionsCommand = new DelegateCommand(this.LoadingUnitsMissionsAppear));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedUnitUnit;
            set
            {
                if (this.SetProperty(ref this.selectedUnitUnit, value))
                {
                    this.LoadingUnitId = this.selectedUnitUnit?.Id;
                    this.RaiseCanExecuteChanged();
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

        public async Task CallLoadingUnitAsync()
        {
            if (!this.loadingUnitId.HasValue)
            {
                this.ShowNotification("Id loading unit does not exists.", Services.Models.NotificationSeverity.Warning);
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.MoveToBayAsync(this.LoadingUnitId.Value);

                this.ShowNotification($"Successfully requested loading unit '{this.SelectedLoadingUnit.Id}'.", Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.LoadingUnitId = null;
                this.IsWaitingForResponse = false;
            }
        }

        public async Task GetLoadingUnitsAsync()
        {
            try
            {
                this.loadingUnits.Clear();
                this.loadingUnits.AddRange(this.machineService.Loadunits);
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

            this.LoadingUnitId = null;

            await this.GetLoadingUnitsAsync();
            this.SelectLoadingUnit();
        }

        public void SelectNextLoadingUnitAsync()
        {
            System.Diagnostics.Debug.Assert(this.currentItemIndex < this.loadingUnits.Count - 1);

            this.currentItemIndex++;
            this.SelectLoadingUnit();
        }

        public void SelectPreviousLoadingUnitAsync()
        {
            System.Diagnostics.Debug.Assert(this.currentItemIndex > 0);

            this.currentItemIndex--;
            this.SelectLoadingUnit();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.callLoadingUnitCommand?.RaiseCanExecuteChanged();
            this.upSelectionCommand?.RaiseCanExecuteChanged();
            this.downSelectionCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCallLoadingUnit()
        {
            return
                this.SelectedLoadingUnit != null
                &&
                this.LoadingUnitId.HasValue
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

        private void CheckToSelectLoadingUnit()
        {
            if (this.loadingUnits.FirstOrDefault(l => l.Id == this.loadingUnitId) is LoadingUnit loadingUnitfound)
            {
                if (loadingUnitfound.Id == this.selectedUnitUnit.Id)
                {
                    return;
                }

                this.currentItemIndex = this.loadingUnits.IndexOf(loadingUnitfound);
                this.SelectLoadingUnit();
            }
            else
            {
                this.currentItemIndex = 0;
                this.SelectLoadingUnit();
            }
        }

        private void LoadingUnitsMissionsAppear()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.LOADINGUNITSMISSIONS,
                null,
                trackCurrentView: true);
        }

        private void SelectLoadingUnit()
        {
            this.SelectedLoadingUnit = this.loadingUnits.ElementAtOrDefault(this.currentItemIndex);
        }

        #endregion
    }
}
