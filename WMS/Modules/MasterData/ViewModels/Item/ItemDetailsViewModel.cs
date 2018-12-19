using System.Threading.Tasks;
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
        private string subTitle;
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
                if (!this.SetProperty(ref this.item, value))
                {
                    return;
                }

                this.TakeSnapshot(this.item);

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

        public string SubTitle { get => this.subTitle; set => this.SetProperty(ref this.subTitle, value); }

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

            this.EvaluateCanExecuteCommands();
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.WithdrawCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemProvider.Save(this.Item);
            if (modifiedRowCount > 0)
            {
                this.TakeSnapshot(this.Item);
                this.SubTitle = this.Item.Code;

                this.EventService.Invoke(new ModelChangedEvent<Item>(this.Item.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemSavedSuccessfully));
            }
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
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
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<Item>>(async eventArgs => await this.LoadData(), this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<Item>>(async eventArgs => await this.LoadData());
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedEvent<Item>>(
                async eventArgs =>
                {
                    if (eventArgs.ModelId.HasValue)
                    {
                        this.Data = eventArgs.ModelId.Value;
                        await this.LoadData();
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

        private async Task LoadData()
        {
            if (this.Data is int modelId)
            {
                this.Item = await this.itemProvider.GetById(modelId);
                this.ItemHasCompartments = this.itemProvider.HasAnyCompartments(modelId);
                this.SubTitle = this.Item.Code;
            }
        }

        #endregion Methods
    }
}
