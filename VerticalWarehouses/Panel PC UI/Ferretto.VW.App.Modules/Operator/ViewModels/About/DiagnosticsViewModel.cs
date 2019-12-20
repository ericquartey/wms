using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class DiagnosticsViewModel : BaseAboutMenuViewModel
    {
        #region Constructors

        public DiagnosticsViewModel()
            : base()
        {
        }

        #endregion
    }
}
