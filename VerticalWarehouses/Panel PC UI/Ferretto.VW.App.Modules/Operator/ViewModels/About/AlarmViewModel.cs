using System.Threading.Tasks;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class AlarmViewModel : BaseAboutMenuViewModel
    {
        #region Constructors

        public AlarmViewModel()
            : base()
        {
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            // TODO: Insert code here

            await base.OnAppearedAsync();
        }

        #endregion
    }
}
