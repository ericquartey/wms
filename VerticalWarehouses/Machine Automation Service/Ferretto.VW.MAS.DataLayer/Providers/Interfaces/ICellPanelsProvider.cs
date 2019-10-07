using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ICellPanelsProvider
    {
        #region Methods

        IEnumerable<CellPanel> GetAll();

        CellPanel UpdateHeight(int cellId, double newHeight);

        #endregion
    }
}
