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
    public class ItemListRowExecuteDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private ICommand executeListRowCommand;

        private ItemListRowExecutionRequest executionRequest;

        private bool isBusy;

        private string validationError;

        #endregion

        #region Constructors

        public ItemListRowExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public string Errors => this.executionRequest.Error;

        public ICommand ExecuteListRowCommand => this.executeListRowCommand ??
            (this.executeListRowCommand = new DelegateCommand(
                async () => await this.ExecuteListRowAsync(),
                this.CanExecuteListRow));

        public ItemListRowExecutionRequest ExecutionRequest
        {
            get => this.executionRequest;
            set
            {
                var oldExecutionRequest = this.executionRequest;

                if (this.SetProperty(ref this.executionRequest, value))
                {
                    if (oldExecutionRequest != null)
                    {
                        oldExecutionRequest.PropertyChanged -= this.OnItemListRowPropertyChanged;
                    }

                    if (this.executionRequest != null)
                    {
                        this.executionRequest.PropertyChanged += this.OnItemListRowPropertyChanged;
                    }
                }
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public string ValidationError
        {
            get => this.validationError;
            set => this.SetProperty(ref this.validationError, value);
        }

        #endregion

        #region Methods

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.executionRequest.ItemListRowDetails = await this.itemListRowProvider.GetByIdAsync(modelId.Value).ConfigureAwait(true);
            this.executionRequest.AreaChoices = await this.areaProvider.GetAllAsync();
            this.executionRequest.PropertyChanged += this.OnAreaIdChanged;
        }

        private bool CanExecuteListRow()
        {
            return string.IsNullOrEmpty(this.executionRequest.Error);
        }

        private async Task ExecuteListRowAsync()
        {
            Debug.Assert(this.executionRequest.AreaId.HasValue, "The parameter must always have a value.");

            this.IsBusy = true;
            IOperationResult<ItemListRow> result = null;
            if (!this.executionRequest.Schedule)
            {
                Debug.Assert(this.executionRequest.BayId.HasValue, "The parameter must always have a value.");

                result = await this.itemListRowProvider.ExecuteImmediatelyAsync(this.executionRequest.ItemListRowDetails.Id, this.executionRequest.AreaId.Value, this.executionRequest.BayId.Value);
            }
            else
            {
                result = await this.itemListRowProvider.ScheduleForExecutionAsync(this.executionRequest.ItemListRowDetails.Id, this.executionRequest.AreaId.Value);
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
                this.ValidationError = result.Description;
            }
        }

        private void Initialize()
        {
            this.ExecutionRequest = new ItemListRowExecutionRequest();
        }

        private async void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.executionRequest.AreaId) &&
                this.executionRequest.AreaId.HasValue)
            {
                this.executionRequest.BayChoices = await this.bayProvider.GetByAreaIdAsync(this.ExecutionRequest.AreaId.Value);
            }
        }

        private void OnItemListRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.ExecuteListRowCommand)?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
