using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Source.Filters;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseLoadingUnitOperationViewModel : BaseMainViewModel
    {
        #region Fields

        private ICommand abortCommand;

        private IEnumerable<TrayControlCompartment> compartments;

        private ICommand confirmCommand;

        private int? inputQuantity;

        private System.Drawing.Image itemImage;

        private MissionOperationInfo missionOperation;

        private TrayControlCompartment selectedCompartment;

        #endregion

        #region Constructors

        public BaseLoadingUnitOperationViewModel(
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

        public ICommand AbortCommand =>
            this.abortCommand
            ??
            (this.abortCommand = new DelegateCommand(async () => await this.ExecuteAbortCommand()));

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmCommand =>
                        this.confirmCommand
                        ??
                        (this.confirmCommand = new DelegateCommand(async () => await this.ExecuteConfirmCommand()));

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

        private async Task ExecuteAbortCommand()
        {
            // TODO add validation
            try
            {
                await this.BayManager.AbortCurrentMissionOperationAsync();

                this.InputQuantity = null;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
