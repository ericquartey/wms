using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.WeightingScale;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
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

        private SampleQuality measuredQuality;

        private double? measuredWeight;

        private double? originalAverageWeight;

        #endregion

        #region Constructors

        public ItemWeightUpdateViewModel(
            IMachineItemsWebService itemsWebService,
            IWeightingScaleService weightingScaleService)
            : base(PresentationMode.Operator)
        {
            this.weightingScaleService = weightingScaleService;
            this.itemsWebService = itemsWebService;

            this.updateWeightCommand = new DelegateCommand(async () => await this.UpdateWeightAsync(), this.CanUpdateWeight);
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

        public SampleQuality MeasuredQuality
        {
            get => this.measuredQuality;
            set => this.SetProperty(ref this.measuredQuality, value, this.RaiseCanExecuteChanged);
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

                //await this.weightingScaleService.StartAsync();
                //this.weightingScaleService.StartWeightAcquisition();
            }
            else
            {
                this.ShowNotification(
                    Resources.ErrorsApp.NoItemLoadedOnPage,
                    Services.Models.NotificationSeverity.Error);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.updateWeightCommand.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }

        private bool CanUpdateWeight()
        {
            return this.MeasuredQuality == SampleQuality.Stable;
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
            if (this.ItemQuantity == 0)
            {
                this.ActualAverageWeight = 0;
                return;
            }

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
                this.ShowNotification(VW.App.Resources.InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
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
