using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Catalog
{
    public class CompartmentsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService filterService;
        private IDataSource<Common.Models.Compartment> currentDataSource;

        private ICommand viewDetailsCommand;

        #endregion Fields

        #region Constructors

        public CompartmentsViewModel()
        {
            this.filterService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<Common.Models.Compartment> CurrentDataSource
        {
            get => this.currentDataSource;
            set => this.SetProperty(ref this.currentDataSource, value);
        }

        public IEnumerable<IDataSource<Common.Models.Compartment>> Filters =>
                    this.filterService.GetAll(MvvmNaming.GetViewNameFromViewModelName(nameof(CompartmentsViewModel))) as IEnumerable<IDataSource<Common.Models.Compartment>>;

        public ICommand ViewDetailsCommand => this.viewDetailsCommand ??
            (this.viewDetailsCommand = new DelegateCommand(ExecuteViewDetailsCommand));

        #endregion Properties

        #region Methods

        private static void ExecuteViewDetailsCommand()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ShowDetailsEventArgs<Common.Models.Compartment>(true));
        }

        #endregion Methods
    }
}
