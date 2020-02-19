﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CellsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private SubscriptionToken cellsToken;

        private int currentIndex;

        private DelegateCommand saveCommand;

        private Cell selectedCell;

        #endregion

        #region Constructors

        public CellsViewModel(
            IMachineCellsWebService machineCellsWebService,
            IHealthProbeService healthProbeService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
        }

        #endregion

        #region Properties

        public IEnumerable<Cell> Cells => this.MachineService.Cells;

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsEnabledEditing => !this.IsMoving;

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.Save(),
                () => !this.IsMoving && this.SelectedCell != null));

        public Cell SelectedCell
        {
            get => this.selectedCell;
            set => this.SetProperty(ref this.selectedCell, value);
        }

        #endregion

        #region Methods

        public void ChangeSelectedItemAsync(bool isUp)
        {
            if (this.Cells == null)
            {
                return;
            }

            if (this.Cells.Any())
            {
                this.currentIndex = isUp ? --this.currentIndex : ++this.currentIndex;
                if (this.currentIndex < 0 || this.currentIndex >= this.Cells.Count())
                {
                    this.currentIndex = (this.currentIndex < 0) ? 0 : this.Cells.Count() - 1;
                }

                this.SelectedCell = this.Cells?.ToList()[this.currentIndex];
            }
        }

        public override void Disappear()
        {
            base.Disappear();

            if (this.cellsToken != null)
            {
                this.EventAggregator.GetEvent<CellsChangedPubSubEvent>().Unsubscribe(this.cellsToken);
                this.cellsToken.Dispose();
                this.cellsToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.SubscribeToEvents();

            if (this.Cells.Any())
            {
                this.SelectedCell = this.Cells?.ToList()[0];
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Cells));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
        }

        private async Task Save()
        {
            try
            {
                await this.machineCellsWebService.SaveCellAsync(this.SelectedCell);

                await this.MachineService.GetCells();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        private void SubscribeToEvents()
        {
            this.cellsToken = this.cellsToken
                 ??
                 this.EventAggregator
                     .GetEvent<CellsChangedPubSubEvent>()
                     .Subscribe(
                         m => this.RaiseCanExecuteChanged(),
                         ThreadOption.UIThread,
                         false);
        }

        #endregion
    }
}
