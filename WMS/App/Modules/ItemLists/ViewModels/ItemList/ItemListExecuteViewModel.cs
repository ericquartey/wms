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
    public class ItemListExecuteViewModel : BaseDialogViewModel<ItemListExecutionRequest>
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ICommand executeListCommand;

        #endregion

        #region Constructors

        public ItemListExecuteViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand ExecuteListCommand => this.executeListCommand ??
            (this.executeListCommand = new DelegateCommand(
                async () => await this.ExecuteListAsync(),
                this.CanExecuteList)
            .ObservesProperty(() => this.Model));

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.ExecuteListCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task OnAppearAsync()
        {
            this.IsBusy = true;

            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync();

            this.IsBusy = false;
        }

        private bool CanExecuteList()
        {
            return !this.IsBusy;
        }

        private async Task ExecuteListAsync()
        {
            if (!this.CheckValidModel())
            {
                return;
            }

            Debug.Assert(this.Model.AreaId.HasValue, "The parameter must always have a value.");

            this.IsBusy = true;
            IOperationResult<ItemList> result = null;
            if (!this.Model.Schedule)
            {
                Debug.Assert(this.Model.BayId.HasValue, "The parameter must always have a value.");

                result = await this.itemListProvider.ExecuteImmediatelyAsync(this.Model.ItemListDetails.Id, this.Model.AreaId.Value, this.Model.BayId.Value);
            }
            else
            {
                result = await this.itemListProvider.ScheduleForExecutionAsync(this.Model.ItemListDetails.Id, this.Model.AreaId.Value);
            }

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.ItemLists.ItemListRequestAccepted, StatusType.Success));
                this.Disappear();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }
        }

        private void Initialize()
        {
            this.Model = new ItemListExecutionRequest();
        }

        private async Task LoadDataAsync()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.Model.ItemListDetails = await this.itemListProvider.GetByIdAsync(modelId.Value).ConfigureAwait(true);
            this.Model.AreaChoices = await this.areaProvider.GetAllAsync();
            this.Model.PropertyChanged += this.OnAreaIdChanged;
        }

        private async void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Model.AreaId) &&
                this.Model.AreaId.HasValue)
            {
                var result = await this.bayProvider.GetByAreaIdAsync(this.Model.AreaId.Value);
                this.Model.BayChoices = result.Success ? result.Entity : null;
            }
        }

        #endregion
    }
}
