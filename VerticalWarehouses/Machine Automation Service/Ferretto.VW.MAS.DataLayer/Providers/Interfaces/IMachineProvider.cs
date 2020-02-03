using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Methods

        void Add(Machine machine);

        void ClearAll();

        Machine Get();

        double GetHeight();

        int GetIdentity();

        MachineStatistics GetStatistics();

        void Import(Machine machine, DataLayerContext context);

        bool IsOneTonMachine();

        void Update(Machine machine, DataLayerContext context);

        void UpdateWeightStatistics(DataLayerContext dataContext);

        #endregion
    }
}
