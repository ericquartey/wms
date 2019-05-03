using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.ItemLists
{
    public class ItemListRowExecuteDialogViewModel : BaseDialogViewModel<ItemListRowExecutionRequest>
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private ICommand executeListRowCommand;

        #endregion

        #region Constructors

        public ItemListRowExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand ExecuteListRowCommand => this.executeListRowCommand ??
            (this.executeListRowCommand = new DelegateCommand(
                async () => await this.ExecuteListRowAsync(),
                this.CanExecuteListRow)
            .ObservesProperty(() => this.Model));

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.ExecuteListRowCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task OnAppearAsync()
        {
            this.IsBusy = true;

            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync();

            this.IsBusy = false;
        }

        private bool CanExecuteListRow()
        {
            return !this.IsBusy;
        }

        private async Task ExecuteListRowAsync()
        {
            if (!this.CheckValidModel())
            {
                return;
            }

            Debug.Assert(this.Model.AreaId.HasValue, "The parameter must always have a value.");

            this.IsBusy = true;
            IOperationResult<ItemListRow> result = null;
            if (!this.Model.Schedule)
            {
                Debug.Assert(this.Model.BayId.HasValue, "The parameter must always have a value.");

                result = await this.itemListRowProvider.ExecuteImmediatelyAsync(this.Model.ItemListRowDetails.Id, this.Model.AreaId.Value, this.Model.BayId.Value);
            }
            else
            {
                result = await this.itemListRowProvider.ScheduleForExecutionAsync(this.Model.ItemListRowDetails.Id, this.Model.AreaId.Value);
            }

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.ItemLists.ItemListRowRequestAccepted, StatusType.Success));
                this.Disappear();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }
        }

        private void Initialize()
        {
            this.Model = new ItemListRowExecutionRequest
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

            this.Model.ItemListRowDetails = await this.itemListRowProvider.GetByIdAsync(modelId.Value).ConfigureAwait(true);
            this.Model.AreaChoices = await this.areaProvider.GetAllAsync();
            this.Model.PropertyChanged += this.OnAreaIdChanged;
            this.Model.IsValidationEnabled = false;

            this.TakeModelSnapshot();
        }

        private async void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Model.AreaId) &&
                this.Model.AreaId.HasValue)
            {
                this.Model.BayChoices = await this.bayProvider.GetByAreaIdAsync(this.Model.AreaId.Value);
            }
        }

        #endregion
    }
}
