using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public abstract class BaseOperatorViewModel : BaseMainViewModel, IRegionMemberLifetime
    {
        #region Constructors

        protected BaseOperatorViewModel(PresentationMode mode)
            : base(mode)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public override bool KeepAlive => true;

        #endregion
    }
}
