using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerCompactingDetailViewModel : BaseMainViewModel
    {
        #region Constructors

        public DrawerCompactingDetailViewModel()
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
