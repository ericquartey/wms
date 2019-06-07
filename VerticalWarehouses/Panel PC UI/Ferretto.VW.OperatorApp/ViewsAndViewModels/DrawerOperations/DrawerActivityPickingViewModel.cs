using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.Utils.Source;
using System;
using Ferretto.VW.Utils.Source.Filters;
using System.Linq;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerActivityPickingViewModel : BindableBase, IDrawerActivityPickingViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private ICommand drawerDetailsButtonCommand;

        private Func<IDrawableCompartment, IDrawableCompartment, string> filterColorFunc;

        private ILoadingUnitsDataService loadingUnitsDataService;

        private TrayControlCompartment selectedCompartment;

        private ObservableCollection<TrayControlCompartment> viewCompartments;

        #endregion

        #region Constructors

        public DrawerActivityPickingViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.filterColorFunc = new EditFilter().ColorFunc;
        }

        #endregion

        #region Properties

        public ICommand DrawerDetailsButtonCommand => this.drawerDetailsButtonCommand ?? (this.drawerDetailsButtonCommand = new DelegateCommand(
            () => NavigationService.NavigateToView<DrawerActivityPickingDetailViewModel, IDrawerActivityPickingDetailViewModel>()));

        public Func<IDrawableCompartment, IDrawableCompartment, string> FilterColorFunc
        {
            get { return this.filterColorFunc; }
            set { this.SetProperty<Func<IDrawableCompartment, IDrawableCompartment, string>>(ref this.filterColorFunc, value); }
        }

        public BindableBase NavigationViewModel { get; set; }

        public TrayControlCompartment SelectedCompartment { get => this.selectedCompartment; set => this.SetProperty(ref this.selectedCompartment, value); }

        public ObservableCollection<TrayControlCompartment> ViewCompartments { get => this.viewCompartments; set => this.SetProperty(ref this.viewCompartments, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.loadingUnitsDataService = this.container.Resolve<ILoadingUnitsDataService>();
        }

        public async Task OnEnterViewAsync()
        {
            var compartments = await this.loadingUnitsDataService.GetCompartmentsAsync(3);
            if (compartments != null && compartments.Count > 0)
            {
                this.ViewCompartments = new ObservableCollection<TrayControlCompartment>(compartments.Select(x => new TrayControlCompartment
                {
                    Height = x.Height,
                    Id = x.Id,
                    LoadingUnitId = x.LoadingUnitId,
                    Width = x.Width,
                    XPosition = x.XPosition,
                    YPosition = x.YPosition
                }));
                this.SelectedCompartment = this.ViewCompartments.First();
            }
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
