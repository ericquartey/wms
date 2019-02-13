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
    public class ItemListRowExecuteDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private ItemListRowExecutionRequest executionRequest;

        private bool isBusy;

        private ICommand runListRowExecuteCommand;

        #endregion

        #region Constructors

        public ItemListRowExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

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

        public ICommand RunListRowExecuteCommand => this.runListRowExecuteCommand ??
                    (this.runListRowExecuteCommand = new DelegateCommand(
                         async () => await this.ExecuteListRowCommandAsync(),
                         this.CanExecuteListRowCommand));

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
            this.executionRequest.AreaChoices = this.areaProvider.GetAll();
            this.executionRequest.PropertyChanged += this.OnAreaIdChanged;
        }

        private bool CanExecuteListRowCommand()
        {
            return string.IsNullOrEmpty(this.executionRequest.Error);
        }

        private async Task ExecuteListRowCommandAsync()
        {
            Debug.Assert(this.executionRequest.AreaId.HasValue, "The parameter must always have a value.");

            this.IsBusy = true;
            OperationResult result = null;
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

            this.EventService.Invoke(result.Success
                ? new StatusPubSubEvent(Common.Resources.MasterData.ListRowRequestAccepted, StatusType.Success)
                : new StatusPubSubEvent(result.Description, StatusType.Error));
        }

        private void Initialize()
        {
            this.ExecutionRequest = new ItemListRowExecutionRequest();
        }

        private void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.executionRequest.AreaId) &&
                this.executionRequest.AreaId.HasValue)
            {
                this.executionRequest.BayChoices = this.bayProvider.GetByAreaId(this.ExecutionRequest.AreaId.Value);
            }
        }

        private void OnItemListRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunListRowExecuteCommand)?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
