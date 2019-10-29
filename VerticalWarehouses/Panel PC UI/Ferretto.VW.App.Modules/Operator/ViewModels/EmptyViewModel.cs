using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class EmptyViewModel : BaseMainViewModel
    {
        #region Constructors

        public EmptyViewModel()
            : base(PresentationMode.Operator)
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
