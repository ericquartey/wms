using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListExecuteDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ItemListExecutionRequest executionRequest;

        private bool isBusy;

        private ICommand runListExecuteCommand;

        private string validationError;

        #endregion

        #region Constructors

        public ItemListExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ItemListExecutionRequest ExecutionRequest
        {
            get => this.executionRequest;
            set
            {
                var oldExecutionRequest = this.executionRequest;

                if (this.SetProperty(ref this.executionRequest, value))
                {
                    if (oldExecutionRequest != null)
                    {
                        oldExecutionRequest.PropertyChanged -= this.OnItemListPropertyChanged;
                    }

                    if (this.executionRequest != null)
                    {
                        this.executionRequest.PropertyChanged += this.OnItemListPropertyChanged;
                    }
                }
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ICommand RunListExecuteCommand => this.runListExecuteCommand ??
                            (this.runListExecuteCommand = new DelegateCommand(
                                 async () => await this.ExecuteListCommandAsync(),
                                 this.CanExecuteListCommand));

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

            this.executionRequest.ItemListDetails = await this.itemListProvider.GetByIdAsync(modelId.Value).ConfigureAwait(true);
            this.executionRequest.AreaChoices = await this.areaProvider.GetAllAsync();
            this.executionRequest.PropertyChanged += this.OnAreaIdChanged;
        }

        private bool CanExecuteListCommand()
        {
            return string.IsNullOrEmpty(this.executionRequest.Error);
        }

        private async Task ExecuteListCommandAsync()
        {
            Debug.Assert(this.executionRequest.AreaId.HasValue, "The parameter must always have a value.");

            this.IsBusy = true;
            OperationResult result = null;
            if (!this.executionRequest.Schedule)
            {
                Debug.Assert(this.executionRequest.BayId.HasValue, "The parameter must always have a value.");

                result = await this.itemListProvider.ExecuteImmediatelyAsync(this.executionRequest.ItemListDetails.Id, this.executionRequest.AreaId.Value, this.executionRequest.BayId.Value);
            }
            else
            {
                result = await this.itemListProvider.ScheduleForExecutionAsync(this.executionRequest.ItemListDetails.Id, this.executionRequest.AreaId.Value);
            }

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ListRequestAccepted, StatusType.Success));
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
            this.ExecutionRequest = new ItemListExecutionRequest();
        }

        private async void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.executionRequest.AreaId) &&
                this.executionRequest.AreaId.HasValue)
            {
                this.executionRequest.BayChoices = await this.bayProvider.GetByAreaIdAsync(this.ExecutionRequest.AreaId.Value);
            }
        }

        private void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunListExecuteCommand)?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
