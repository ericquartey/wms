using System.Linq;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
#if DEBUG
using System.IO;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
#else
using Ferretto.Common.BusinessProviders;
#endif

namespace Ferretto.WMS.Modules.MasterData
{
    [Module(ModuleName = nameof(Common.Utils.Modules.MasterData), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MasterDataModule : IModule
    {
        #region Constructors

        public MasterDataModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.NavigationService = navigationService;
        }

        #endregion Constructors

        #region Properties

        public IUnityContainer Container { get; private set; }
        public INavigationService NavigationService { get; private set; }
        public IRegionManager RegionManager { get; private set; }

        #endregion Properties

        #region Methods

        public void Initialize()
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingMasterDataModule);

            this.NavigationService.Register<ItemsView, ItemsViewModel>();
            this.NavigationService.Register<ItemDetailsView, ItemDetailsViewModel>();

            this.NavigationService.Register<CellsView, CellsViewModel>();
            this.NavigationService.Register<CellDetailsView, CellDetailsViewModel>();

            this.NavigationService.Register<CompartmentsView, CompartmentsViewModel>();
            this.NavigationService.Register<CompartmentDetailsView, CompartmentDetailsViewModel>();

            this.NavigationService.Register<LoadingUnitsView, LoadingUnitsViewModel>();
            this.NavigationService.Register<LoadingUnitDetailsView, LoadingUnitDetailsViewModel>();
            this.NavigationService.Register<LoadingUnitEditView, LoadingUnitEditViewModel>();

            this.NavigationService.Register<WithdrawDialogView, WithdrawDialogViewModel>();

            this.NavigationService.Register<InputAddCompartmentView, InputAddCompartmentViewModel>();
            this.NavigationService.Register<InputBulkAddCompartmentView, InputBulkAddCompartmentViewModel>();
            this.NavigationService.Register<InputEditCompartmentView, InputEditCompartmentViewModel>();

            this.NavigationService.Register<ListsView, ListsViewModel>();
            //this.NavigationService.Register<ItemDetailsView, ItemDetailsViewModel>();

#if DEBUG
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.CheckingDatabaseStructure);

            var dataContext = ServiceLocator.Current.GetInstance<DatabaseContext>();
            try
            {
                var pendingMigrations = dataContext.Database.GetPendingMigrations();
                if (pendingMigrations.Any())
                {
                    SplashScreenService.SetMessage(Common.Resources.DesktopApp.ApplyingDatabaseMigrations);
                    dataContext.Database.Migrate();

                    SplashScreenService.SetMessage(Common.Resources.DesktopApp.ReseedingDatabase);
                    dataContext.Database.ExecuteSqlCommand(File.ReadAllText(@"bin\Debug\net471\Seeds\Dev.Minimal.sql"));
                    dataContext.Database.ExecuteSqlCommand(File.ReadAllText(@"bin\Debug\net471\Seeds\Dev.Items.sql"));
                }
            }
            catch
            {
                SplashScreenService.SetMessage(Common.Resources.Errors.UnableToConnectToDatabase);
            }
#else

            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingEntityFramework);
            ServiceLocator.Current.GetInstance<IItemProvider>().GetAll().ToList();
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.DoneInitializingEntityFramework);

#endif
        }

        #endregion Methods
    }
}
