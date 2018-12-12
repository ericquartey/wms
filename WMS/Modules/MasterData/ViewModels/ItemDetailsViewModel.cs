using System.ComponentModel;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : DetailsViewModel<ItemDetails>, IRefreshDataEntityViewModel, IEdit
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();
        private IDataSource<Compartment> compartmentsDataSource;
        private ItemDetails item;
        private bool itemHasCompartments;
        private object modelChangedEventSubscription;
        private object modelRefreshSubscription;
        private object modelSelectionChangedSubscription;
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
                if (this.item == value)
                {
                    return;
                }

                if (this.item != null)
                {
                    this.item.PropertyChanged -= this.OnItemPropertyChanged;
                }

                this.SetProperty(ref this.item, value);

                this.ChangeDetector.TakeSnapshot(this.item);

                this.item.PropertyChanged += this.OnItemPropertyChanged;

                this.RefreshData();
            }
        }

        public bool ItemHasCompartments
        {
            get => this.itemHasCompartments;
            set => this.SetProperty(ref this.itemHasCompartments, value);
        }

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
            this.CompartmentsDataSource = this.item != null
                ? new DataSource<Compartment>(() => this.compartmentProvider.GetByItemId(this.item.Id))
                : null;
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.WithdrawCommand)?.RaiseCanExecuteChanged();
        }

        protected override void ExecuteRevertCommand()
        {
            this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemProvider.Save(this.Item);
            if (modifiedRowCount > 0)
            {
                this.ChangeDetector.TakeSnapshot(this.Item);

                this.EventService.Invoke(new ModelChangedEvent<Item>(this.Item.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemSavedSuccessfully));
            }
        }

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<Item>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<Item>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedEvent<Item>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private bool CanExecuteWithdraw()
        {
            return this.Item?.TotalAvailable > 0;
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
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<Item>>(eventArgs => this.LoadData(), this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<Item>>(eventArgs => this.LoadData());
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
            this.EvaluateCanExecuteCommands();
        }

        #endregion Methods
    }
}
