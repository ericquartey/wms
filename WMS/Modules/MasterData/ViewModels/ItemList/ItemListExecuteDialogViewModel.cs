using System.ComponentModel;
using System.Diagnostics;
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

        #endregion Fields

        #region Constructors

        public ItemListExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ItemListExecutionRequest ExecutionRequest
        {
            get => this.executionRequest;
            set
            {
                if (this.executionRequest == value)
                {
                    return;
                }

                if (this.executionRequest != null)
                {
                    this.executionRequest.PropertyChanged -= this.OnItemListPropertyChanged;
                }

                if (this.SetProperty(ref this.executionRequest, value))
                {
                    this.ExecutionRequest.PropertyChanged += this.OnItemListPropertyChanged;
                }
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ICommand RunListExecuteCommand => this.runListExecuteCommand ??
                            (this.runListExecuteCommand = new DelegateCommand(this.ExecuteListCommand, this.CanExecuteListCommand));

        public string ValidationError
        {
            get => this.validationError;
            set => this.SetProperty(ref this.validationError, value);
        }

        #endregion Properties

        #region Methods

        protected override async void OnAppear()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.executionRequest.ItemListDetails = await this.itemListProvider.GetById(modelId.Value);
            this.executionRequest.AreaChoices = this.areaProvider.GetAll();
            this.executionRequest.PropertyChanged += new PropertyChangedEventHandler(this.OnAreaIdChanged);
        }

        private bool CanExecuteListCommand()
        {
            return string.IsNullOrEmpty(this.executionRequest.Error);
        }

        private async void ExecuteListCommand()
        {
            Debug.Assert(this.executionRequest.AreaId.HasValue);

            this.IsBusy = true;
            OperationResult result = null;
            if (!this.executionRequest.Schedule)
            {
                Debug.Assert(this.executionRequest.BayId.HasValue);

                result = await this.itemListProvider.ExecuteImmediately(this.executionRequest.ItemListDetails.Id, this.executionRequest.AreaId.Value, this.executionRequest.BayId.Value);
            }
            else
            {
                result = await this.itemListProvider.ScheduleForExecution(this.executionRequest.ItemListDetails.Id, this.executionRequest.AreaId.Value);
            }

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ListRequestAccepted, StatusType.Success));
                this.Disappear();
            }
            else
            {
                this.EventService.Invoke(new StatusEventArgs(result.Description, StatusType.Error));
                this.ValidationError = result.Description;
            }
        }

        private void Initialize()
        {
            this.ExecutionRequest = new ItemListExecutionRequest();
        }

        private void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.executionRequest.AreaId) &&
                this.executionRequest.AreaId.HasValue)
            {
                this.executionRequest.BayChoices = this.bayProvider.GetByAreaId(this.ExecutionRequest.AreaId.Value);
            }
        }

        private void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunListExecuteCommand)?.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
