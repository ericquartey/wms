using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerActivityLocalCallViewModel : BaseDrawerOperationViewModel
    {
        #region Constructors

        public DrawerActivityLocalCallViewModel(
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(wmsDataProvider, wmsImagesProvider, missionOperationsService, bayManager)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        #endregion
    }
}
