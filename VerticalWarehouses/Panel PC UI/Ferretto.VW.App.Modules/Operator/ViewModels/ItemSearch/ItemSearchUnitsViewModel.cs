using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public struct ItemInUnits
    {
        #region Fields

        public ItemInfo Item;

        public LoadingUnit Unit;

        #endregion
    }

    [Warning(WarningsArea.Picking)]
    public class ItemSearchUnitsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private readonly IWmsDataProvider wmsDataProvider;

        private bool isBusyRecallLoadingUnit;

        private ItemInfo item;

        private IEnumerable<Compartment> itemUnits;

        private DelegateCommand recallLoadingUnitCommand;

        private Compartment selectedItemUnits;

        #endregion

        #region Constructors

        public ItemSearchUnitsViewModel(
            INavigationService navigationService,
            IMissionOperationsService missionOperationsService,
            IWmsDataProvider wmsDataProvider,
            IMachineItemsWebService itemsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemsSearch.ToString();

        public bool IsBusyRecallLoadingUnit
        {
            get => this.isBusyRecallLoadingUnit;
            set => this.SetProperty(ref this.isBusyRecallLoadingUnit, value, this.RaiseCanExecuteChanged);
        }

        public ItemInfo Item
        {
            get => this.item;
            set
            {
                if (value is null)
                {
                    this.RaisePropertyChanged();
                    return;
                }

                this.SetProperty(ref this.item, value);
            }
        }

        public IEnumerable<Compartment> ItemUnits
        {
            get => this.itemUnits;
            set => this.SetProperty(ref this.itemUnits, value, this.RaiseCanExecuteChanged);
        }

        public ICommand RecallLoadingUnitCommand =>
            this.recallLoadingUnitCommand
            ??
            (this.recallLoadingUnitCommand = new DelegateCommand(
                async () => await this.RecallLoadingUnitAsync(),
                this.CanRecallLoadingUnit));

        public Compartment SelectedItemUnits
        {
            get => this.selectedItemUnits;
            set => this.SetProperty(ref this.selectedItemUnits, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.Item = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;

            this.ItemUnits = await this.itemsWebService.GetCompartmentsAsync(this.Item.Id);

            this.RaisePropertyChanged(nameof(this.ItemUnits));
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsBusyRecallLoadingUnit = true;
                this.IsWaitingForResponse = true;

                var activeOperation = this.missionOperationsService.ActiveWmsOperation;
                this.Logger.Debug($"User requested recall of loading unit.");

                if (activeOperation != null)
                {
                    var canComplete = await this.missionOperationsService.CompleteAsync(activeOperation.Id, 1);
                    if (!canComplete)
                    {
                        this.Logger.Debug($"Operation '{activeOperation.Id}' cannot be completed, forcing recall of loading unit.");

                        await this.missionOperationsService.RecallLoadingUnitAsync(this.SelectedItemUnits.LoadingUnitId);
                    }
                }
                else
                {
                    await this.missionOperationsService.RecallLoadingUnitAsync(this.SelectedItemUnits.LoadingUnitId);
                }

                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.WAIT);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsBusyRecallLoadingUnit = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.recallLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanRecallLoadingUnit()
        {
            return
                !this.isBusyRecallLoadingUnit
                &&
                (this.SelectedItemUnits?.LoadingUnitId != null);
        }

        #endregion
    }
}
