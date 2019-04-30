using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemWithdrawDialogViewModel : BaseDialogViewModel<ItemWithdraw>
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private bool advancedWithdraw;

        private ICommand runWithdrawCommand;

        #endregion

        #region Constructors

        public ItemWithdrawDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand RunWithdrawCommand => this.runWithdrawCommand ??
            (this.runWithdrawCommand = new DelegateCommand(
                    async () => await this.RunWithdrawAsync(),
                    this.CanRunWithdraw)
                .ObservesProperty(() => this.Model)
                .ObservesProperty(() => this.Model.Quantity));

        public bool AdvancedWithdraw
        {
            get => this.advancedWithdraw;
            set
            {
                this.SetProperty(ref this.advancedWithdraw, value);
                if (!this.advancedWithdraw)
                {
                    this.Model.Lot = null;
                    this.Model.RegistrationNumber = null;
                    this.Model.Sub1 = null;
                    this.Model.Sub2 = null;
                }
            }
        }

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
        }

        protected override async void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Model_PropertyChanged(sender, e);
            if (e == null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(this.Model.AreaId):
                    this.Model.BayChoices = this.Model.AreaId.HasValue
                        ? await this.bayProvider.GetByAreaIdAsync(this.Model.AreaId.Value)
                        : null;
                    break;

                case nameof(this.Model.ItemDetails):
                    this.Model.AreaChoices = this.Model.ItemDetails != null
                        ? await this.areaProvider.GetAreasWithAvailabilityAsync(this.Model.ItemDetails.Id)
                        : null;
                    break;
            }
        }

        protected override async Task OnAppearAsync()
        {
            this.IsBusy = true;

            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync();

            this.IsBusy = false;
        }

        private bool CanRunWithdraw()
        {
            return !this.IsBusy;
        }

        private bool CheckWithdrawConditions()
        {
            this.Model.IsValidationEnabled = true;

            return this.IsModelValid
                && this.ChangeDetector.IsRequiredValid;
        }

        private void Initialize()
        {
            this.Model = new ItemWithdraw
            {
                IsValidationEnabled = false
            };
        }

        private async Task LoadDataAsync()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.Model.ItemDetails = await this.itemProvider.GetByIdAsync(modelId.Value).ConfigureAwait(true);
        }

        private async Task RunWithdrawAsync()
        {
            if (!this.CheckWithdrawConditions())
            {
                return;
            }

            this.IsBusy = true;

            var result = await this.itemProvider.WithdrawAsync(this.Model);

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.MasterData.ItemWithdrawCommenced,
                    StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    result.Description,
                    StatusType.Error));
            }

            this.IsBusy = false;
        }

        #endregion
    }
}
