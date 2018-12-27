using System;
using System.ComponentModel;
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
        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ItemListExecutionRequest executionRequest;

        private ICommand runListRowExecuteCommand;

        #endregion Fields

        #region Constructors

        public ItemListRowExecuteDialogViewModel()
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
                if (this.executionRequest != null && value != this.executionRequest)
                {
                    this.executionRequest.PropertyChanged -= this.OnItemListRowPropertyChanged;
                }
                if (this.SetProperty(ref this.executionRequest, value))
                {
                    this.executionRequest.PropertyChanged += this.OnItemListRowPropertyChanged;
                }
            }
        }

        public ICommand RunListRowExecuteCommand => this.runListRowExecuteCommand ??
                    (this.runListRowExecuteCommand = new DelegateCommand(this.ExecuteListRowCommand));

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.executionRequest.ItemListDetails = new ItemListDetails();
            this.executionRequest.AreaChoices = this.areaProvider.GetAll();
            this.executionRequest.PropertyChanged += new PropertyChangedEventHandler(this.OnAreaIdChanged);
        }

        private void ExecuteListRowCommand()
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            this.ExecutionRequest = new ItemListExecutionRequest();
        }

        private void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ExecutionRequest.AreaId) &&
                this.ExecutionRequest.AreaId.HasValue)
            {
                this.ExecutionRequest.BayChoices = this.bayProvider.GetByAreaId(this.ExecutionRequest.AreaId.Value);
            }
        }

        private void OnItemListRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunListRowExecuteCommand)?.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
