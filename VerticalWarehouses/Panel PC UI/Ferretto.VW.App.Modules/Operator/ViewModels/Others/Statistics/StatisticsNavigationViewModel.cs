using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    public class StatisticsNavigationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private MachineStatistics model;

        #endregion

        #region Constructors

        public StatisticsNavigationViewModel()
            : base(PresentationMode.Operator)
        {
            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public MachineStatistics Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.RaisePropertyChanged(nameof(this.MenuItems));
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Statistics.MACHINE,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.StatisticsNavigationMachine,
                    trackCurrentView: true));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Statistics.Drawers.WEIGHTSATURATION,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.StatisticsNavigationDrawers,
                    trackCurrentView: true));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Statistics.CELLS,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.StatisticsNavigationCells,
                    trackCurrentView: true));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Operator.Others.Statistics.ERRORS,
                    nameof(Utils.Modules.Operator),
                    VW.App.Resources.OperatorApp.StatisticsNavigationErrors,
                    trackCurrentView: true));
        }

        #endregion
    }
}
