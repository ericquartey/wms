using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IStatisticsFooterViewModel : IViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion
    }
}
