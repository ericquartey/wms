using System.Linq;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using NLog;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "This class associate all Views to related ViewModels")]
    [Module(ModuleName = nameof(Common.Utils.Modules.MasterData), OnDemand = true)]
    [ModuleDependency(nameof(Common.Utils.Modules.BusinessLogic))]
    public class MasterDataModule : IModule
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region Constructors

        public MasterDataModule(IUnityContainer container, IRegionManager regionManager, INavigationService navigationService)
        {
            this.Container = container;
            this.RegionManager = regionManager;
            this.NavigationService = navigationService;
        }

        #endregion

        #region Properties

        public IUnityContainer Container { get; private set; }

        public INavigationService NavigationService { get; private set; }

        public IRegionManager RegionManager { get; private set; }

        #endregion

        #region Methods

        public void Initialize()
        {
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.InitializingMasterDataModule);

            this.logger.Trace("Loading module ...");

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
            this.NavigationService.Register<ItemListExecuteDialogView, ItemListExecuteDialogViewModel>();
            this.NavigationService.Register<ItemListRowExecuteDialogView, ItemListRowExecuteDialogViewModel>();

            this.NavigationService.Register<ItemListsView, ItemListsViewModel>();
            this.NavigationService.Register<ItemListDetailsView, ItemListDetailsViewModel>();

            this.NavigationService.Register<ItemListRowDetailsView, ItemListRowDetailsViewModel>();

            this.NavigationService.Register<FilterDialogView, FilterDialogViewModel>();

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

#pragma warning disable S1481 // Remove the unused local variable 'dbInitValue'
            var dbInitValue = ServiceLocator.Current.GetInstance<IItemProvider>().GetAll().ToList();
#pragma warning restore S1481 //  Remove the unused local variable 'dbInitValue'
            SplashScreenService.SetMessage(Common.Resources.DesktopApp.DoneInitializingEntityFramework);

#endif

            this.logger.Trace("Module loaded.");
        }

        #endregion
    }
}
