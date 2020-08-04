﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels.ItemOperations
{
    public class ItemWeightUpdateViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private readonly DelegateCommand updateWeightCommand;

        private readonly IWeightingScaleService weightingScaleService;

        private double? actualAverageWeight;

        private string itemCode;

        private int itemId;

        private int itemQuantity;

        private double? measuredWeight;

        private double? originalAverageWeight;

        #endregion

        #region Constructors

        public ItemWeightUpdateViewModel(
            PresentationMode mode,
            IMachineItemsWebService itemsWebService,
            IWeightingScaleService weightingScaleService)
            : base(mode)
        {
            this.weightingScaleService = weightingScaleService;
            this.itemsWebService = itemsWebService;

            this.updateWeightCommand = new DelegateCommand(async () => await this.UpdateWeightAsync());
        }

        #endregion

        #region Properties

        public double? ActualAverageWeight
        {
            get => this.actualAverageWeight;
            private set => this.SetProperty(ref this.actualAverageWeight, value);
        }

        public string ItemCode
        {
            get => this.itemCode;
            private set => this.SetProperty(ref this.itemCode, value);
        }

        public int ItemQuantity
        {
            get => this.itemQuantity;
            set => this.SetProperty(ref this.itemQuantity, value, this.UpdateActualAverageWeight);
        }

        public double? MeasuredWeight
        {
            get => this.measuredWeight;
            set => this.SetProperty(ref this.measuredWeight, value, this.UpdateActualAverageWeight);
        }

        public double? OriginalAverageWeight
        {
            get => this.originalAverageWeight;
            private set => this.SetProperty(ref this.originalAverageWeight, value);
        }

        public ICommand UpdateWeightCommand => this.updateWeightCommand;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            if (this.Data is int itemId)
            {
                await this.LoadItemData(itemId);

                await this.weightingScaleService.StartAsync();
                this.weightingScaleService.StartWeightAcquisition();
            }
            else
            {
                this.ShowNotification(
                    Resources.ErrorsApp.NoItemLoadedOnPage,
                    Services.Models.NotificationSeverity.Error);
            }
        }

        private async Task LoadItemData(int itemId)
        {
            this.itemId = itemId;

            this.IsWaitingForResponse = true;

            try
            {
                var item = await this.itemsWebService.GetByIdAsync(this.itemId);

                this.ItemCode = item.Code;
                this.OriginalAverageWeight = item.AverageWeight;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void UpdateActualAverageWeight()
        {
            this.ActualAverageWeight = this.ItemQuantity > 0
                ? this.MeasuredWeight / this.ItemQuantity
                : 0;
        }

        private async Task UpdateWeightAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.weightingScaleService.UpdateItemAverageWeightAsync(this.itemId, this.actualAverageWeight);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
