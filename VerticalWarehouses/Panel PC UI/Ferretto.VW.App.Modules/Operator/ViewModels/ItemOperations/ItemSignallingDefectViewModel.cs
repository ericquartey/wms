using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemSignallingDefectViewModel : BaseOperatorViewModel // BaseItemOperationMainViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private double availableQuantity;

        private bool canInputGoodDraperyQuantity;

        private bool canInputWastedDraperyQuantity;

        private DelegateCommand confirmSignallingDefectOperationCommand;

        private string draperyItemDescription;

        private double? goodDraperyQuantity;

        private bool isConfirmSignallingDefectButtonEnabled;

        private int itemId;

        private MissionWithLoadingUnitDetails mission;

        private MissionOperation missionOperation;

        private double quantityIncrement;

        private int? quantityTolerance;

        private int selectedCompartmentId;

        private double? wastedDraperyQuantity;

        #endregion

        #region Constructors

        public ItemSignallingDefectViewModel(
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IMachineLoadingUnitsWebService loadingUnitsWebService)
            : base(PresentationMode.Operator)
        {
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));
        }

        #endregion

        #region Properties

        public double AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value);
        }

        public bool CanInputGoodDraperyQuantity
        {
            get => this.canInputGoodDraperyQuantity;
            protected set => this.SetProperty(ref this.canInputGoodDraperyQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool CanInputWastedDraperyQuantity
        {
            get => this.canInputWastedDraperyQuantity;
            protected set => this.SetProperty(ref this.canInputWastedDraperyQuantity, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ConfirmSignallingDefectOperationCommand =>
                  this.confirmSignallingDefectOperationCommand
                  ??
                  (this.confirmSignallingDefectOperationCommand = new DelegateCommand(
                      async () => await this.ConfirmSignallingDefectOperationAsync(),
                      this.CanConfirmSignallingDefectButton));

        public string DraperyItemDescription
        {
            get => this.draperyItemDescription;
            set => this.SetProperty(ref this.draperyItemDescription, value, this.RaiseCanExecuteChanged);
        }

        public double? GoodDraperyQuantity
        {
            get => this.goodDraperyQuantity;
            set => this.SetProperty(ref this.goodDraperyQuantity, value, () =>
            {
                this.IsConfirmSignallingDefectButtonEnabled = value.HasValue &&
                    (value + this.wastedDraperyQuantity <= this.availableQuantity) &&
                    value >= 0 &&
                    ((this.wastedDraperyQuantity > 0 && value >= 0) || (this.wastedDraperyQuantity == 0 && value > 0));
            });
        }

        public bool IsConfirmSignallingDefectButtonEnabled
        {
            get => this.isConfirmSignallingDefectButtonEnabled;
            protected set => this.SetProperty(ref this.isConfirmSignallingDefectButtonEnabled, value, this.RaiseCanExecuteChanged);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance.Value);
                }
            }
        }

        public double? WastedDraperyQuantity
        {
            get => this.wastedDraperyQuantity;
            set => this.SetProperty(ref this.wastedDraperyQuantity, value, () =>
            {
                this.IsConfirmSignallingDefectButtonEnabled = value.HasValue &&
                    (this.goodDraperyQuantity + value <= this.availableQuantity) &&
                    value >= 0 &&
                    ((this.goodDraperyQuantity > 0 && value >= 0) || (this.goodDraperyQuantity == 0 && value > 0));
            });
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            if (this.Data is MissionOperation missionOperation &&
                this.missionOperationsService.ActiveWmsMission != null)
            {
                await this.LoadItemDataAsync(missionOperation.ItemId);
            }

            this.IsConfirmSignallingDefectButtonEnabled = false;
            this.CanInputGoodDraperyQuantity = true;
            this.CanInputWastedDraperyQuantity = true;

            await base.OnAppearedAsync();

            this.IsWaitingForResponse = false;
        }

        private static IEnumerable<TrayControlCompartment> MapCompartments(IEnumerable<CompartmentMissionInfo> compartmentsFromMission)
        {
            try
            {
                return compartmentsFromMission
                    .Where(c =>
                        c.Width.HasValue
                        ||
                        c.Depth.HasValue
                        ||
                        c.XPosition.HasValue
                        ||
                        c.YPosition.HasValue)
                    .Select(c => new TrayControlCompartment
                    {
                        Depth = c.Depth.Value,
                        Id = c.Id,
                        Width = c.Width.Value,
                        XPosition = c.XPosition.Value,
                        YPosition = c.YPosition.Value,
                        Barcode = c.Barcode,
                    });
            }
            catch (Exception)
            {
                return Array.Empty<TrayControlCompartment>();
            }
        }

        private bool CanConfirmSignallingDefectButton()
        {
            return this.isConfirmSignallingDefectButtonEnabled;
        }

        private async Task ConfirmSignallingDefectOperationAsync()
        {
            var isItemDeleted = this.GoodDraperyQuantity + this.WastedDraperyQuantity == this.availableQuantity;

            var bResult = true;

            this.ClearNotifications();

            try
            {
                this.ShowNotification(Localized.Get("OperatorApp.SignallingDefectOnProcess"), Services.Models.NotificationSeverity.Info);

                bResult = await this.itemsWebService.SignallingDefectOnDraperyItemAsync(
                    this.missionOperation.ItemBarcode,
                    this.GoodDraperyQuantity.Value,
                    this.WastedDraperyQuantity.Value);

                // Notification messages
                if (!bResult)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.SignallingDefectFailed"), Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    if (isItemDeleted)
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.ItemIsDeleted"), Services.Models.NotificationSeverity.Warning);
                        this.IsConfirmSignallingDefectButtonEnabled = false;
                    }
                    else
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.SignallingDefectSuccess"), Services.Models.NotificationSeverity.Success);
                    }
                }

                if (bResult && !isItemDeleted)
                {
                    // Go back to the Pick view
                    this.NavigationService.GoBack();
                }
            }
            catch (Exception exc)
            {
                this.Logger.Debug($"Signalling defect operation. Error: {exc}");
                this.ShowNotification(Localized.Get("OperatorApp.SignallingDefectError"), Services.Models.NotificationSeverity.Error);
            }
        }

        private async Task LoadItemDataAsync(int itemId)
        {
            this.itemId = itemId;
            this.mission = this.missionOperationsService.ActiveWmsMission;
            this.missionOperation = this.missionOperationsService.ActiveWmsOperation;

            this.GoodDraperyQuantity = 0;
            this.WastedDraperyQuantity = 0;

            try
            {
                var compartments = MapCompartments(this.mission.LoadingUnit.Compartments);
                var selectedCompartment = compartments.SingleOrDefault(c =>
                    c.Id == this.missionOperation.CompartmentId);
                this.selectedCompartmentId = selectedCompartment.Id;

                var loadingUnitId = this.mission.LoadingUnit.Id;
                //var unit = await this.missionOperationsWebService.GetUnitIdAsync(this.Mission.Id);
                var itemsCompartments = await this.loadingUnitsWebService.GetCompartmentsAsync(loadingUnitId);
                itemsCompartments = itemsCompartments?.Where(ic => !(ic.ItemId is null));
                var selectedCompartmentDetail = itemsCompartments.Where(s => s.Id == selectedCompartment.Id && s.ItemId == this.itemId).FirstOrDefault();

                this.AvailableQuantity = (double)selectedCompartmentDetail?.Stock;

                var item = await this.itemsWebService.GetByIdAsync(this.itemId);
                this.DraperyItemDescription = this.missionOperation.ItemDescription;
                this.QuantityTolerance = item.PickTolerance ?? 0;
            }
            catch (Exception)
            {
                this.DraperyItemDescription = string.Empty;
                this.AvailableQuantity = 0;
                this.AvailableQuantity = 500;
                this.QuantityTolerance = 1;
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.confirmSignallingDefectOperationCommand.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
