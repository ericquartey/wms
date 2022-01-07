using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.WeightingScale;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemWeightViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly DelegateCommand addCommand;

        private readonly DelegateCommand cancelCommand;

        private readonly DelegateCommand confirmMeasuredQtyCommand;

        private readonly DelegateCommand confirmRequestedQtyCommand;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly DelegateCommand resetCommand;

        private readonly DelegateCommand updateAverageWeightCommand;

        private readonly IWeightingScaleService weightingScaleService;

        private double? averageWeight;

        private string itemCode;

        private int itemId;

        private SampleQuality measuredQuality;

        private int? measuredQuantity;

        private double requestedQuantity;

        private int? totalMeasuredQuantity;

        private float weight;

        #endregion

        #region Constructors

        public ItemWeightViewModel(
            IMachineItemsWebService itemsWebService,
            IWeightingScaleService weightingScaleService)
            : base(PresentationMode.Operator)
        {
            this.itemsWebService = itemsWebService;
            this.weightingScaleService = weightingScaleService;

            this.cancelCommand = new DelegateCommand(this.Cancel);
            this.updateAverageWeightCommand = new DelegateCommand(this.UpdateAverageWeight);
            this.confirmMeasuredQtyCommand = new DelegateCommand(this.ConfirmMeasuredQty, this.CanConfirmMeasuredQty);
            this.confirmRequestedQtyCommand = new DelegateCommand(this.ConfirmRequestedQty);
            this.addCommand = new DelegateCommand(this.AddMeasuredQty, this.CanAddMeasuredQty);
            this.resetCommand = new DelegateCommand(this.ResetMeasuredQty, this.CanReset);
        }

        #endregion

        #region Properties

        public ICommand AddCommand => this.addCommand;

        public double? AverageWeight
        {
            get => this.averageWeight;
            set => this.SetProperty(ref this.averageWeight, value);
        }

        public ICommand ConfirmMeasuredQtyCommand => this.confirmMeasuredQtyCommand;

        public ICommand ConfirmRequestedQtyCommand => this.confirmRequestedQtyCommand;

        public string ItemCode
        {
            get => this.itemCode;
            set => this.SetProperty(ref this.itemCode, value);
        }

        public SampleQuality MeasuredQuality
        {
            get => this.measuredQuality;
            set => this.SetProperty(ref this.measuredQuality, value, this.RaiseCanExecuteChanged);
        }

        public int? MeasuredQuantity
        {
            get => this.measuredQuantity;
            set => this.SetProperty(ref this.measuredQuantity, value, this.RaiseCanExecuteChanged);
        }

        public double RequestedQuantity
        {
            get => this.requestedQuantity;
            set => this.SetProperty(ref this.requestedQuantity, value);
        }

        public ICommand ResetCommand => this.resetCommand;

        public int? TotalMeasuredQuantity
        {
            get => this.totalMeasuredQuantity;
            set => this.SetProperty(ref this.totalMeasuredQuantity, value);
        }

        public ICommand UpdateAverageWeightCommand => this.updateAverageWeightCommand;

        public float Weight
        {
            get => this.weight;
            set
            {
                if (value > 0)
                {
                    this.MeasuredQuantity = (int?)Math.Round((this.averageWeight.HasValue && this.averageWeight.Value != 0) ? value / this.averageWeight.Value : value);
                }

                if (this.MeasuredQuantity < 0)
                {
                    this.MeasuredQuantity = 0;
                }

                this.SetProperty(ref this.weight, value, this.RaiseCanExecuteChanged);
            }
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            if (this.Data is MissionOperation missionOperation)
            {
                this.RequestedQuantity = missionOperation.RequestedQuantity;
                await this.LoadItemDataAsync(missionOperation.ItemId);
            }

            await base.OnAppearedAsync();

            this.IsWaitingForResponse = false;
        }

        private void AddMeasuredQty()
        {
            if (this.MeasuredQuantity.HasValue)
            {
                if (this.TotalMeasuredQuantity.HasValue)
                {
                    this.TotalMeasuredQuantity += this.MeasuredQuantity;
                }
                else
                {
                    this.TotalMeasuredQuantity = this.MeasuredQuantity;
                }
            }

            this.RaiseCanExecuteChanged();
        }

        private bool CanAddMeasuredQty()
        {
            return this.measuredQuality == SampleQuality.Stable
                &&
                this.measuredQuantity.HasValue
                &&
                this.measuredQuantity.Value > 0;
        }

        private void Cancel()
        {
            this.NavigationService.GoBack();
        }

        private bool CanConfirmMeasuredQty()
        {
            if (((this.totalMeasuredQuantity.HasValue && this.totalMeasuredQuantity.Value > 0)
                ||
                (this.measuredQuantity.HasValue && this.measuredQuantity.Value > 0))
                &&
                this.measuredQuality == SampleQuality.Stable)
            {
                return true;
            }

            return false;
        }

        private bool CanReset()
        {
            return this.totalMeasuredQuantity.HasValue;
        }

        private void ConfirmMeasuredQty()
        {
            if (!this.measuredQuantity.HasValue
                &&
                !this.TotalMeasuredQuantity.HasValue)
            {
                return;
            }

            var newQuantity = (this.TotalMeasuredQuantity ?? 0) + (this.measuredQuantity ?? 0);

            var msg = new ItemWeightChangedMessage(newQuantity, null);
            this.EventAggregator.GetEvent<PubSubEvent<ItemWeightChangedMessage>>().Publish(msg);

            this.TotalMeasuredQuantity = null;
            this.NavigationService.GoBack();
        }

        private void ConfirmRequestedQty()
        {
            var msg = new ItemWeightChangedMessage(null, this.requestedQuantity);
            this.EventAggregator.GetEvent<PubSubEvent<ItemWeightChangedMessage>>().Publish(msg);

            this.TotalMeasuredQuantity = null;

            this.NavigationService.GoBack();
        }

        private async Task LoadItemDataAsync(int itemId)
        {
            this.itemId = itemId;

            try
            {
                var item = await this.itemsWebService.GetByIdAsync(this.itemId);

                this.ItemCode = item.Code;
                if (item.UnitWeight.HasValue)
                {
                    this.AverageWeight = item.UnitWeight;
                }
                else
                {
                    this.AverageWeight = item.AverageWeight;
                }

                if (this.AverageWeight.HasValue && this.AverageWeight > 0)
                {
                    await this.weightingScaleService.SetAverageUnitaryWeightAsync((float)this.AverageWeight.Value);
                }
                else
                {
                    await this.weightingScaleService.ResetAverageUnitaryWeightAsync();
                }
            }
            catch (Exception ex)
            {
                this.ItemCode = null;
                this.AverageWeight = null;
                this.ShowNotification(ex);
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.updateAverageWeightCommand.RaiseCanExecuteChanged();
            this.confirmMeasuredQtyCommand.RaiseCanExecuteChanged();
            this.confirmRequestedQtyCommand.RaiseCanExecuteChanged();
            this.resetCommand.RaiseCanExecuteChanged();
            this.addCommand.RaiseCanExecuteChanged();
        }

        private void ResetMeasuredQty()
        {
            this.TotalMeasuredQuantity = null;
        }

        private void UpdateAverageWeight()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.WEIGHT_UPDATE,
                this.itemId);
        }

        #endregion
    }
}
