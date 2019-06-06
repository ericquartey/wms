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
            this.ViewCompartments = new ObservableCollection<TrayControlCompartment>();
            for (int i = 0; i < compartments.Count; i++)
            {
                this.ViewCompartments.Add(new TrayControlCompartment
                {
                    Height = compartments[i].Height,
                    Id = compartments[i].Id,
                    LoadingUnitId = compartments[i].LoadingUnitId,
                    Width = compartments[i].Width,
                    XPosition = compartments[i].XPosition,
                    YPosition = compartments[i].YPosition
                });
            }
            this.SelectedCompartment = this.ViewCompartments[0];
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
