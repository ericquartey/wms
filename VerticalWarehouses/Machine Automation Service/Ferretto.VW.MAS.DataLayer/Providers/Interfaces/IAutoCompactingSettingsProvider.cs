using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IAutoCompactingSettingsProvider
    {
        #region Methods

        void AddAutoCompactingSettings(AutoCompactingSettings autoCompactingSettings);

        void AddOrModifyAutoCompactingSettings(AutoCompactingSettings autoCompactingSettings);

        IEnumerable<AutoCompactingSettings> GetAllAutoCompactingSettings();

        void ModifyAutoCompactingSettings(AutoCompactingSettings newAutoCompactingSettings);

        void RemoveAutoCompactingSettingsById(int id);

        public void UpdateStatus();

        #endregion
    }
}
