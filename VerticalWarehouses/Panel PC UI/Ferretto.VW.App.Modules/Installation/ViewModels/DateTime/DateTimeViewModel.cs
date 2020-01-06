using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class DateTimeViewModel : BaseMainViewModel
    {
        #region Constructors

        public DateTimeViewModel(PresentationMode mode)
            : base(mode)
        {
        }

        #endregion
    }
}
