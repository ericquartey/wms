﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private bool canInputQuantity;

        private string currentItemHeight;

        private string measureUnit;

        private string measureUnitDescription;

        private MissionWithLoadingUnitDetails mission;

        private MissionOperation missionOperation;

        private int onMissionOperationItemCodeFontSize;

        private double quantityIncrement;

        private int? quantityTolerance;

        private CompartmentDetails selectedCompartmentDetail;

        #endregion

        #region Constructors

        public BaseItemOperationViewModel(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IBayManager bayManager,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.loadingUnitsWebService = loadingUnitsWebService;
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.DialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            protected set => this.SetProperty(ref this.canInputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public string CurrentItemHeight
        {
            get => this.currentItemHeight;
            set => this.SetProperty(ref this.currentItemHeight, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public string ItemId => this.missionOperation?.ItemId.ToString();

        public IMachineItemsWebService ItemsWebService => this.itemsWebService;

        public string MeasureUnit
        {
            get => this.measureUnit;
            set => this.SetProperty(ref this.measureUnit, value);
        }

        public string MeasureUnitDescription
        {
            get => this.measureUnitDescription;
            set => this.SetProperty(ref this.measureUnitDescription, value);
        }

        public MissionWithLoadingUnitDetails Mission
        {
            get => this.mission;
            private set => this.SetProperty(ref this.mission, value);
        }

        public MissionOperation MissionOperation
        {
            get => this.missionOperation;
            private set => this.SetProperty(ref this.missionOperation, value);
        }

        public int OnMissionOperationItemCodeFontSize
        {
            get => this.onMissionOperationItemCodeFontSize;
            set => this.SetProperty(ref this.onMissionOperationItemCodeFontSize, value);
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

        public CompartmentDetails SelectedCompartmentDetail
        {
            get => this.selectedCompartmentDetail;
            set => this.SetProperty(ref this.selectedCompartmentDetail, value);
        }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        protected IBayManager BayManager { get; }

        protected IDialogService DialogService { get; private set; }

        protected IMissionOperationsService MissionOperationsService { get; }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.MissionOperation = null;
            this.Mission = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.RetrieveMissionOperationAsync();

            this.CanInputQuantity = true;

            this.IsBackNavigationAllowed = true;
        }

        public virtual void OnMisionOperationRetrieved()
        {
            // do nothing
        }

        public async Task SuspendOperationAsync()
        {
            this.ClearNotifications();

            var messageBoxResult = this.DialogService.ShowMessage(
               $"{this.missionOperation.ItemListCode} - {this.missionOperation.ItemListRowCode} - {this.MissionOperation.ItemCode}",
               Localized.Get("InstallationApp.SuspendOperation"),
               DialogType.Question,
               DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.ShowNotification(Localized.Get("OperatorApp.Suspending"), Services.Models.NotificationSeverity.Info);

                    var bResult = await this.MissionOperationsService.SuspendAsync(this.MissionOperation.Id);

                    // Notification messages
                    if (!bResult)
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.SuspendOperationFailed"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.SuspendOperationSuccess"), Services.Models.NotificationSeverity.Success);

                        // Go back to the Pick/Put view
                        this.NavigationService.GoBack();
                    }
                }
                catch (Exception exc)
                {
                    this.Logger.Debug($"Suspend operation. Error: {exc}");
                    this.ShowNotification(Localized.Get("OperatorApp.SuspendOperationError"), Services.Models.NotificationSeverity.Error);
                }
            }
        }

        protected async Task RetrieveMissionOperationAsync()
        {
            if (this.MissionOperationsService.ActiveWmsOperation is null)
            {
                // ?????????????? this.NavigationService.GoBack();
                this.NavigationService.GoBackTo(
                   nameof(Utils.Modules.Operator),
                   Utils.Modules.Operator.ItemOperations.WAIT,
                   "RetrieveMissionOperationAsync 1");
                return;
            }

            this.Mission = this.MissionOperationsService.ActiveWmsMission;
            this.MissionOperation = this.MissionOperationsService.ActiveWmsOperation;

            if (this.MissionOperation.ItemHeight.HasValue)
            {
                this.CurrentItemHeight = this.MissionOperation.ItemHeight.Value.ToString();
            }
            else
            {
                this.CurrentItemHeight = "--";
            }

            this.OnMissionOperationItemCodeFontSize = this.SelectFontSize(this.MissionOperation.ItemCode);

            //if (this.missionOperation.Type == MissionOperationType.LoadingUnitCheck
            //    && string.IsNullOrEmpty(this.missionOperation.ItemCode))
            //{
            //    this.NavigationService.GoBackTo(
            //       nameof(Utils.Modules.Operator),
            //       Utils.Modules.Operator.ItemOperations.WAIT,
            //       "RetrieveMissionOperationAsync 2");
            //    return;
            //}

            try
            {
                if (this.MissionOperation?.ItemId > 0)
                {
                    var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                    this.QuantityTolerance = item.PickTolerance ?? 0;
                    this.MeasureUnit = item.MeasureUnitDescription;
                }

                if (this.Mission is null)
                {
                    this.NavigationService.GoBackTo(
                       nameof(Utils.Modules.Operator),
                       Utils.Modules.Operator.ItemOperations.WAIT,
                       "RetrieveMissionOperationAsync 3");
                    return;
                }

                var itemsCompartments = await this.loadingUnitsWebService.GetCompartmentsAsync(this.Mission.LoadingUnit.Id);
                itemsCompartments = itemsCompartments?.Where(ic => !(ic.ItemId is null));
                if (this.missionOperation is null)
                {
                    this.NavigationService.GoBackTo(
                       nameof(Utils.Modules.Operator),
                       Utils.Modules.Operator.ItemOperations.WAIT,
                       "RetrieveMissionOperationAsync 4");
                    return;
                }

                this.selectedCompartmentDetail = itemsCompartments.FirstOrDefault(s => s.Id == this.missionOperation.CompartmentId && s.ItemId == this.MissionOperation.ItemId);

                this.RaisePropertyChanged(nameof(this.SelectedCompartmentDetail));
                this.RaisePropertyChanged(nameof(this.ItemId));
                this.RaisePropertyChanged(nameof(this.MeasureUnit));
                this.RaisePropertyChanged(nameof(this.QuantityTolerance));

                this.RaisePropertyChanged(nameof(this.OnMissionOperationItemCodeFontSize));

                this.OnMisionOperationRetrieved();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                //this.NavigationService.GoBackTo(
                //   nameof(Utils.Modules.Operator),
                //   Utils.Modules.Operator.ItemOperations.WAIT);
                this.ShowNotification(ex);
            }
        }

        private int SelectFontSize(string value)
        {
            // Simple mapping
            const int _FontSize_Very_Small = 10;
            const int _FontSize_Small = 14;
            const int _FontSize_Normal = 16;
            const int _FontSize_High = 18;
            const int _FontSize_Very_High = 28;

            var fontSize = _FontSize_Very_High;

            if (value != null)
            {
                var len = value.Length;
                if (len >= 25)
                {
                    fontSize = _FontSize_Very_Small;
                }
                else if (len >= 20)
                {
                    fontSize = _FontSize_Normal;
                }
                else if (len >= 12)
                {
                    fontSize = _FontSize_High;
                }
            }

            return fontSize;
        }

        #endregion
    }
}
