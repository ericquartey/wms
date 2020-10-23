using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchUnitsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private DelegateCommand callLoadingUnitCommand;

        private ItemInfo item;

        private IEnumerable<Compartment> itemUnits;

        private Compartment selectedItemUnits;

        #endregion

        #region Constructors

        public ItemSearchUnitsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineItemsWebService itemsWebService)
            : base(PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemsSearch.ToString();

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

        public ICommand LoadingUnitCallCommand =>
            this.callLoadingUnitCommand
            ??
            (this.callLoadingUnitCommand = new DelegateCommand(
                async () => await this.CallLoadingUnitAsync(),
                this.CanCallLoadingUnit));

        public Compartment SelectedItemUnits
        {
            get => this.selectedItemUnits;
            set => this.SetProperty(ref this.selectedItemUnits, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public async Task CallLoadingUnitAsync()
        {
            if (this.SelectedItemUnits?.LoadingUnitId == null)
            {
                this.ShowNotification(Resources.Localized.Get("General.IdLoadingUnitNotExists"), Services.Models.NotificationSeverity.Warning);
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.MoveToBayAsync(this.SelectedItemUnits.LoadingUnitId);

                this.ShowNotification(string.Format(Resources.Localized.Get("ServiceMachine.LoadingUnitSuccessfullyRequested"), this.SelectedItemUnits.LoadingUnitId), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.SelectedItemUnits = null;
                this.IsWaitingForResponse = false;
            }
        }

        public override void Disappear()
        {
            this.Item = null;

            this.ItemUnits = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.NoteEnabled = true;
            this.RaisePropertyChanged(nameof(this.NoteEnabled));

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;

            this.ItemUnits = await this.itemsWebService.GetCompartmentsAsync(this.Item.Id);

            this.RaisePropertyChanged(nameof(this.ItemUnits));

            if (this.ItemUnits.Any())
            {
                this.SelectedItemUnits = this.ItemUnits.FirstOrDefault();
                this.RaisePropertyChanged(nameof(this.SelectedItemUnits));
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.callLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCallLoadingUnit()
        {
            return
                this.SelectedItemUnits?.LoadingUnitId != null
                &&
                !this.IsWaitingForResponse;
        }

        #endregion
    }
}
