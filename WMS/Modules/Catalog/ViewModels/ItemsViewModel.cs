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
    public class ItemsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService filterService;
        private IDataSource<Common.Models.Item> currentDataSource;

        private ICommand viewDetailsCommand;

        #endregion Fields

        #region Constructors

        public ItemsViewModel()
        {
            this.filterService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<IDataSource<Common.Models.Item>> Filters =>
            this.filterService.GetAll(MvvmNaming.GetViewNameFromViewModelName(nameof(ItemsViewModel))) as IEnumerable<IDataSource<Common.Models.Item>>;

        public ICommand ViewDetailsCommand => this.viewDetailsCommand ??
            (this.viewDetailsCommand = new DelegateCommand(ExecuteViewDetailsCommand));

        public IDataSource<Common.Models.Item> CurrentDataSource
        {
            get => this.currentDataSource;
            set => this.SetProperty(ref this.currentDataSource, value);
        }

        #endregion Properties

        #region Methods

        private static void ExecuteViewDetailsCommand()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ShowDetailsEventArgs<Common.Models.Item>(true));
        }

        #endregion Methods
    }
}
