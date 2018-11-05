using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
        private IDataSource<Compartment> compartmentsDataSource;
        private ItemDetails item;
        private bool itemHasCompartments;
        private object modelSelectionChangedSubscription;
        private ICommand revertCommand;
        private ICommand saveCommand;
        private object selectedCompartment;
        private ICommand withdrawCommand;

        #endregion Fields

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<Compartment> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public Compartment CurrentCompartment
        {
            get
            {
                if (this.selectedCompartment == null)
                {
                    return default(Compartment);
                }
                if ((this.selectedCompartment is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(Compartment);
                }
                return (Compartment)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedCompartment).OriginalRow);
            }
        }

        public ItemDetails Item
        {
            get => this.item;
            set
            {
                if (this.Item != null && value != this.Item)
                {
                    this.Item.PropertyChanged -= this.OnItemPropertyChanged;
                }

                if (!this.SetProperty(ref this.item, value))
                {
                    return;
                }

                this.Item.PropertyChanged += this.OnItemPropertyChanged;

                this.RefreshData();
            }
        }

        public bool ItemHasCompartments
        {
            get => this.itemHasCompartments;
            set => this.SetProperty(ref this.itemHasCompartments, value);
        }

        public ICommand RevertCommand => this.revertCommand ??
                  (this.revertCommand = new DelegateCommand(this.LoadData));

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand));

        public object SelectedCompartment
        {
            get => this.selectedCompartment;
            set
            {
                this.SetProperty(ref this.selectedCompartment, value);
                this.RaisePropertyChanged(nameof(this.CurrentCompartment));
            }
        }

        public ICommand WithdrawCommand => this.withdrawCommand ??
                                          (this.withdrawCommand = new DelegateCommand(this.ExecuteWithdraw,
                                              this.CanExecuteWithdraw));

        #endregion Properties

        #region Methods

        public void RefreshData()
        {
            this.CompartmentsDataSource = null;
            this.CompartmentsDataSource = this.item != null
                ? this.dataSourceService
                    .GetAll<Compartment>(nameof(ItemDetailsViewModel), this.item.Id)
                    .Single()
                : null;
        }

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Item>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private bool CanExecuteWithdraw()
        {
            return this.Item?.TotalAvailable > 0;
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemProvider.Save(this.Item);

            if (modifiedRowCount > 0)
            {
                this.EventService.Invoke(new ModelChangedEvent<Item>(this.Item.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemSavedSuccessfully));
            }
        }

        private void ExecuteWithdraw()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.WITHDRAWDIALOG,
                new
                {
                    Id = this.Item.Id
                }
            );
        }

        private void Initialize()
        {
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Item>>(
                eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        this.LoadData();
                    }
                    else
                    {
                        this.Item = null;
                    }
                },
                this.Token,
                true,
                true);
        }

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.Item = this.itemProvider.GetById(modelId);
                this.ItemHasCompartments = this.itemProvider.HasAnyCompartments(modelId);
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.Item.TotalAvailable))
            {
                ((DelegateCommand)this.WithdrawCommand)?.RaiseCanExecuteChanged();
            }
        }

        #endregion Methods
    }
}
