﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CellsSideControlViewModel : BaseMainViewModel, IDataErrorInfo
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private IEnumerable<Cell> cells;

        private DelegateCommand correctCommand;

        private int? inputFormCellId;

        private int? inputToCellId;

        private bool isBackActive;

        private bool isFrontActive;

        private DelegateCommand sideBackCommand;

        private DelegateCommand sideFrontCommand;

        private WarehouseSide sideSelected;

        private double? stepValue;

        #endregion

        #region Constructors

        public CellsSideControlViewModel(
            IMachineCellsWebService machineCellsWebService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
        }

        #endregion

        #region Properties

        public IEnumerable<Cell> Cells
        {
            get => this.cells;
            private set
            {
                if (this.SetProperty(ref this.cells, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand CorrectCommand =>
                    this.correctCommand
            ??
            (this.correctCommand = new DelegateCommand(
                async () => await this.CorrectCommandAsync(),
                this.CanCorrectCommand));

        public string Error => string.Join(
            this[nameof(this.StepValue)],
            this[nameof(this.InputFormCellId)],
            this[nameof(this.InputToCellId)]);

        public int? InputFormCellId
        {
            get => this.inputFormCellId;
            set
            {
                if (this.SetProperty(ref this.inputFormCellId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int? InputToCellId
        {
            get => this.inputToCellId;
            set
            {
                if (this.SetProperty(ref this.inputToCellId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBackActive
        {
            get => this.isBackActive;
            private set => this.SetProperty(ref this.isBackActive, value);
        }

        public bool IsFrontActive
        {
            get => this.isFrontActive;
            private set => this.SetProperty(ref this.isFrontActive, value);
        }

        public override bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    if (this.isWaitingForResponse)
                    {
                        this.ClearNotifications();
                    }
                }
            }
        }

        public ICommand SideBackCommand =>
            this.sideBackCommand
            ??
            (this.sideBackCommand = new DelegateCommand(
                this.ToggleSide,
                this.CanToggleToBackSide));

        public ICommand SideFrontCommand =>
            this.sideFrontCommand
            ??
            (this.sideFrontCommand = new DelegateCommand(
                this.ToggleSide,
                this.CanToggleToFrontSide));

        public double? StepValue
        {
            get => this.stepValue;
            set
            {
                if (this.SetProperty(ref this.stepValue, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.StepValue):
                        if (!this.StepValue.HasValue)
                        {
                            return Localized.Get("InstallationApp.StepRequired");
                        }

                        if (this.StepValue.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.StepMustBePositive");
                        }

                        break;

                    case nameof(this.InputFormCellId):
                        if (!this.InputFormCellId.HasValue)
                        {
                            return Localized.Get("InstallationApp.InputFormCellIdRequired");
                        }

                        if (this.InputFormCellId.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.InputFormCellIdMustBePositive");
                        }

                        break;

                    case nameof(this.InputToCellId):
                        if (!this.InputToCellId.HasValue)
                        {
                            return Localized.Get("InstallationApp.InputToCellIdRequired");
                        }

                        if (this.InputToCellId.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.InputToCellIdMustBePositive");
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.RetrieveCellsAsync();

            this.IsBackNavigationAllowed = true;

            this.sideSelected = WarehouseSide.Back;

            this.ToggleSide();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.sideBackCommand?.RaiseCanExecuteChanged();
            this.sideFrontCommand?.RaiseCanExecuteChanged();
            this.correctCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCorrectCommand()
        {
            return string.IsNullOrEmpty(this.Error)
                &&
                !this.isWaitingForResponse
                &&
                this.cells != null
                &&
                this.stepValue.HasValue
                &&
                this.inputToCellId.HasValue
                &&
                this.inputFormCellId.HasValue;
        }

        private bool CanToggleToBackSide()
        {
            return
                !this.isWaitingForResponse
                &&
                this.sideSelected == WarehouseSide.Front;
        }

        private bool CanToggleToFrontSide()
        {
            return
                !this.isWaitingForResponse
                &&
                this.sideSelected == WarehouseSide.Back;
        }

        private async Task CorrectCommandAsync()
        {
            try
            {
                await this.machineCellsWebService.UpdatesHeightAsync(this.inputFormCellId.Value, this.inputToCellId.Value, this.sideSelected, this.stepValue.Value);

                this.ShowNotification(Localized.Get("InstallationApp.SuccessfullChange"), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task RetrieveCellsAsync()
        {
            try
            {
                this.Cells = await this.machineCellsWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void ToggleSide()
        {
            if (this.sideSelected == WarehouseSide.Front)
            {
                this.sideSelected = WarehouseSide.Back;
                this.IsFrontActive = false;
                this.IsBackActive = true;
            }
            else
            {
                this.sideSelected = WarehouseSide.Front;
                this.IsFrontActive = true;
                this.IsBackActive = false;
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
