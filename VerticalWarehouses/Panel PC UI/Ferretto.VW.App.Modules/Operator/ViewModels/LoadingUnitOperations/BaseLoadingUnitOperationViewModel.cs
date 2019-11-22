using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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

        private readonly IMissionsDataService missionDataService;

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
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.WmsImagesProvider = wmsImagesProvider ?? throw new ArgumentNullException(nameof(wmsImagesProvider));
            this.BayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.missionDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));

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

        public Image ItemImage
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

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        protected IBayManager BayManager { get; }

        protected IMachineMissionOperationsWebService MissionOperationsService { get; }

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

                    await this.CheckOtherOperationsOnSameMissionAsync();
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
            await this.GetTrayControlDataAsync();

            this.IsBackNavigationAllowed = true;
        }

        private async Task CheckOtherOperationsOnSameMissionAsync()
        {
            try
            {
                var mission = await this.missionDataService.GetDetailsByIdAsync(this.BayManager.CurrentMission.Id);
                if (mission.Operations?.Any(o => o.Status == MissionOperationStatus.New) == true)
                {
                    this.IsEnabled = false;
                    this.InputQuantity = null;
                    this.ItemImage = null;
                }
                else
                {
                    this.NavigationService.GoBack();
                    return;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task ExecuteAbortCommand()
        {
            var abortResult = await this.BayManager.AbortCurrentMissionOperationAsync();
            if (abortResult)
            {
                this.NavigationService.GoBack();
            }

            this.InputQuantity = null;
        }

        private IEnumerable<TrayControlCompartment> GetCompartments(ObservableCollection<CompartmentMissionInfo> compartmentsFromMission)
        {
            var compartments = new List<TrayControlCompartment>();
            foreach (var comp in compartmentsFromMission)
            {
                if (comp.Width.HasValue
                    ||
                    comp.Depth.HasValue
                    ||
                    comp.XPosition.HasValue
                    ||
                    comp.YPosition.HasValue)
                {
                    var newComp = new TrayControlCompartment
                    {
                        Depth = comp.Depth.Value,
                        Id = comp.Id,
                        Width = comp.Width.Value,
                        XPosition = comp.XPosition.Value,
                        YPosition = comp.YPosition.Value,
                    };
                    compartments.Add(newComp);
                }
            }

            return compartments;
        }

        private async Task GetTrayControlDataAsync()
        {
            try
            {
                var mission = await this.missionDataService.GetDetailsByIdAsync(this.BayManager.CurrentMission.Id);
                this.Compartments = this.GetCompartments(mission.LoadingUnit.Compartments);
                this.SelectedCompartment = this.Compartments
                        .FirstOrDefault(c => c.Id == this.BayManager.CurrentMissionOperation.CompartmentId);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task LoadImage(string code)
        {
            this.ItemImage?.Dispose();
            this.ItemImage = null;
            var stream = await this.WmsImagesProvider.GetImageAsync(code);
            this.ItemImage = Image.FromStream(stream);
        }

        #endregion
    }
}
