using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
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

        public virtual bool KeepAlive => false;

        #endregion
    }
}
