using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseDrawerOperationViewModel : BaseMainViewModel
    {
        #region Fields

        private IEnumerable<TrayControlCompartment> compartments;

        private int? inputQuantity;

        private System.Drawing.Image itemImage;

        private MissionOperationInfo missionOperation;

        private TrayControlCompartment selectedCompartment;

        #endregion

        #region Constructors

        public BaseDrawerOperationViewModel(
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.WmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
            this.WmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.MissionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));

            this.CompartmentColoringFunction = new EditFilter().ColorFunc;
        }

        #endregion

        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public int? InputQuantity { get => this.inputQuantity; set => this.SetProperty(ref this.inputQuantity, value); }

        public System.Drawing.Image ItemImage
        {
            get => this.itemImage;
            set
            {
                var oldImage = this.itemImage;
                if (this.SetProperty(ref this.itemImage, value))
                {
                    oldImage?.Dispose();
                }
            }
        }

        public MissionOperationInfo MissionOperation
        {
            get => this.missionOperation;
            set => this.SetProperty(ref this.missionOperation, value);
        }

        public TrayControlCompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        protected IBayManager BayManager { get; }

        protected IMachineMissionOperationsWebService MissionOperationsService { get; }

        protected IWmsDataProvider WmsDataProvider { get; }

        protected IWmsImagesProvider WmsImagesProvider { get; }

        #endregion

        #region Methods

        public virtual async Task ExecuteConfirmCommand()
        {
            // TODO add validation
            if (this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= 0)
            {
                try
                {
                    await this.BayManager.CompleteCurrentMissionOperationAsync(this.InputQuantity.Value);

                    this.InputQuantity = null;

                    this.UpdateView();
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.UpdateView();
        }

        public virtual void UpdateView()
        {
            var missionOperation = this.BayManager.CurrentMissionOperation;
            if (missionOperation != null)
            {
                switch (missionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.DrawerOperations.INVENTORY,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Pick:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.DrawerOperations.PICKING,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Put:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.DrawerOperations.REFILLING,
                            null,
                            trackCurrentView: true);
                        break;
                }
            }
            else
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.DrawerOperations.WAIT,
                    null,
                    trackCurrentView: true);
            }
        }

        protected async Task GetTrayControlDataAsync(IBayManager bayManager)
        {
            try
            {
                this.Compartments = await this.WmsDataProvider.GetTrayControlCompartmentsAsync(bayManager.CurrentMission);
                this.SelectedCompartment = this.Compartments
                    .FirstOrDefault(c => c.Id == bayManager.CurrentMissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
