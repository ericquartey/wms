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
    public class ListExecuteDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();
        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();
        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ListToExecute listToExecute;

        private ICommand runListExecuteCommand;

        #endregion Fields

        #region Constructors

        public ListExecuteDialogViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ListToExecute ListToExecute
        {
            get => this.listToExecute;
            set
            {
                if (this.ListToExecute != null && value != this.ListToExecute)
                {
                    this.ListToExecute.PropertyChanged -= this.OnItemListPropertyChanged;
                }
                if (this.SetProperty(ref this.listToExecute, value))
                {
                    this.ListToExecute.PropertyChanged += this.OnItemListPropertyChanged;
                }
            }
        }

        public ICommand RunListExecuteCommand => this.runListExecuteCommand ??
                    (this.runListExecuteCommand = new DelegateCommand(this.ExecuteListCommand));

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("Id")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            this.ListToExecute.ItemListDetails = this.itemListProvider.GetById(modelId.Value);
            this.ListToExecute.AreaChoices = this.areaProvider.GetAll();
            this.listToExecute.PropertyChanged += new PropertyChangedEventHandler(this.OnAreaIdChanged);
        }

        private void ExecuteListCommand()
        {
            throw new NotImplementedException();
        }

        private void Initialize()
        {
            this.ListToExecute = new ListToExecute();
        }

        private void OnAreaIdChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ListToExecute.AreaId) &&
                this.ListToExecute.AreaId.HasValue)
            {
                this.ListToExecute.BayChoices = this.bayProvider.GetByAreaId(this.ListToExecute.AreaId.Value);
            }
        }

        private void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunListExecuteCommand)?.RaiseCanExecuteChanged();
        }

        #endregion Methods
    }
}
