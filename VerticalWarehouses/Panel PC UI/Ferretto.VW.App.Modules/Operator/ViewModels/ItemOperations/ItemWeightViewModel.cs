using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
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

        private float? averageWeight;

        private string itemCode;

        private int itemId;

        private int? measuredQuantity;

        private double requestedQuantity;

        private int? totalMeasuredQuantity;

        private float weight;

        #endregion

        #region Constructors

        public ItemWeightViewModel(IMachineItemsWebService itemsWebService)
            : base(PresentationMode.Operator)
        {
            this.cancelCommand = new DelegateCommand(this.Cancel);
            this.updateAverageWeightCommand = new DelegateCommand(this.UpdateAverageWeight);
            this.confirmMeasuredQtyCommand = new DelegateCommand(this.ConfirmMeasuredQty, this.CanConfirmMeasuredQty);
            this.confirmRequestedQtyCommand = new DelegateCommand(this.ConfirmRequestedQty);
            this.addCommand = new DelegateCommand(this.AddMeasuredQty);
            this.resetCommand = new DelegateCommand(this.ResetMeasuredQty, this.CanReset);
            this.itemsWebService = itemsWebService;
        }

        #endregion

        #region Properties

        public ICommand AddCommand => this.addCommand;

        public float? AverageWeight
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

        public int? MeasuredQuantity
        {
            get => this.measuredQuantity;
            set => this.SetProperty(ref this.measuredQuantity, value);
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
            set => this.SetProperty(ref this.weight, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            if (this.Data is MissionOperation missionOperation)
            {
                this.RequestedQuantity = missionOperation.RequestedQuantity;
                await this.LoadItemData(missionOperation.ItemId);
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

        private void Cancel()
        {
            this.NavigationService.GoBack();
        }

        private bool CanConfirmMeasuredQty()
        {
            return this.measuredQuantity.HasValue;
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

            var newQuantity = (this.TotalMeasuredQuantity.HasValue) ? this.TotalMeasuredQuantity : this.measuredQuantity;
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

        private async Task LoadItemData(int itemId)
        {
            this.itemId = itemId;

            try
            {
                var item = await this.itemsWebService.GetByIdAsync(this.itemId);

                this.ItemCode = item.Code;
                this.AverageWeight = item.AverageWeight;
            }
            catch (Exception ex)
            {
                this.ItemCode = "123 test";
                this.AverageWeight = 0.3f;
                this.ShowNotification(ex);
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.updateAverageWeightCommand.RaiseCanExecuteChanged();
            this.confirmMeasuredQtyCommand.RaiseCanExecuteChanged();
            this.confirmRequestedQtyCommand.RaiseCanExecuteChanged();
            this.resetCommand.RaiseCanExecuteChanged();
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
