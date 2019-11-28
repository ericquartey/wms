using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerCompactingDetailViewModel : BaseOperatorViewModel
    {
        #region Constructors

        public DrawerCompactingDetailViewModel()
            : base(PresentationMode.Operator)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

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
