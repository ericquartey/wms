using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ISetupProceduresDataProvider
    {
        #region Methods

        SetupProceduresSet GetAll();

        #endregion
    }
}
