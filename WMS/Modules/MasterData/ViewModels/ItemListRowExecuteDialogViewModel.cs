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

        private ListToExecute listRowToExecute;

        private ICommand runListRowExecuteCommand;

        #endregion Fields

        #region Constructors

        public ItemListRowExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ListToExecute ListRowToExecute
        {
            get => this.listRowToExecute;
            set
            {
                if (this.ListRowToExecute != null && value != this.ListRowToExecute)
                {
                    this.ListRowToExecute.PropertyChanged -= this.OnItemListRowPropertyChanged;
                }
                if (this.SetProperty(ref this.listRowToExecute, value))
                {
                    this.ListRowToExecute.PropertyChanged += this.OnItemListRowPropertyChanged;
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

            this.ListRowToExecute.ItemListDetails = new ItemListDetails(); 
            this.ListRowToExecute.AreaChoices = this.areaProvider.GetAll();
            this.ListRowToExecute.PropertyChanged += new PropertyChangedEventHandler(this.OnAreaIdChanged);
        }

        private void ExecuteListRowCommand()
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            this.ListRowToExecute = new ListToExecute();
        }

        private void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ListRowToExecute.AreaId) &&
                this.ListRowToExecute.AreaId.HasValue)
            {
                this.ListRowToExecute.BayChoices = this.bayProvider.GetByAreaId(this.ListRowToExecute.AreaId.Value);
            }
        }

        private void OnItemListRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunListRowExecuteCommand)?.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
