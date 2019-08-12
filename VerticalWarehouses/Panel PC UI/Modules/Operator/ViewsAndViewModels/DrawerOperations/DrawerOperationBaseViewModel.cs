using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels
{
    public class DrawerOperationBaseViewModel : BaseViewModel
    {
        #region Fields

        private IEnumerable<TrayControlCompartment> compartments;

        private int? inputQuantity;

        private System.Drawing.Image itemImage;

        private MissionOperationInfo missionOperation;

        private TrayControlCompartment selectedCompartment;

        #endregion

        #region Constructors

        public DrawerOperationBaseViewModel(
            IEventAggregator eventAggregator,
            Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService navigationService,
            IStatusMessageService statusMessageService,
            IMainWindowViewModel mainWindowViewModel,
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsService missionOperationsService,
            IBayManager bayManager)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            if (statusMessageService == null)
            {
                throw new ArgumentNullException(nameof(statusMessageService));
            }

            if (mainWindowViewModel == null)
            {
                throw new ArgumentNullException(nameof(mainWindowViewModel));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            if (wmsImagesProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsImagesProvider));
            }

            if (missionOperationsService == null)
            {
                throw new ArgumentNullException(nameof(missionOperationsService));
            }

            if (bayManager == null)
            {
                throw new ArgumentNullException(nameof(bayManager));
            }

            this.CompartmentColoringFunction = new EditFilter().ColorFunc;
            this.EventAggregator = eventAggregator;
            this.NavigationService = navigationService;
            this.StatusMessageService = statusMessageService;
            this.MainWindowViewModel = mainWindowViewModel;
            this.WmsDataProvider = wmsDataProvider;
            this.WmsImagesProvider = wmsImagesProvider;
            this.MissionOperationsService = missionOperationsService;
            this.BayManager = bayManager;
        }

        #endregion

        #region Properties

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

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

        protected IEventAggregator EventAggregator { get; }

        protected IMainWindowViewModel MainWindowViewModel { get; }

        protected IMachineMissionOperationsService MissionOperationsService { get; }

        protected Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService NavigationService { get; }

        protected IStatusMessageService StatusMessageService { get; }

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
                    this.StatusMessageService.Notify(ex);
                }
            }
        }

        public void UpdateView()
        {
            var missionOperation = this.BayManager.CurrentMissionOperation;
            var mainWindowContentVM = this.MainWindowViewModel.ContentRegionCurrentViewModel;
            if (mainWindowContentVM is DrawerActivityInventoryViewModel ||
                mainWindowContentVM is DrawerActivityPickingViewModel ||
                mainWindowContentVM is DrawerActivityRefillingViewModel ||
                mainWindowContentVM is DrawerWaitViewModel)
            {
                if (missionOperation != null)
                {
                    switch (missionOperation.Type)
                    {
                        case MissionOperationType.Inventory:
                            this.NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionOperationType.Pick:
                            this.NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionOperationType.Put:
                            this.NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;
                    }
                }
                else
                {
                    this.NavigationService.NavigateToViewWithoutNavigationStack<DrawerWaitViewModel, IDrawerWaitViewModel>();
                }
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
                throw new ApplicationException(ex.Message);
            }
        }

        #endregion
    }
}
