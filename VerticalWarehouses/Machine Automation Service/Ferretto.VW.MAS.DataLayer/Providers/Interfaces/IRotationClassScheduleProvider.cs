using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IRotationClassScheduleProvider
    {
        #region Methods

        void AddOrModifyRotationClassSchedule(RotationClassSchedule rotationClassSchedule);

        bool CheckRotationClass();

        IEnumerable<RotationClassSchedule> GetAllRotationClassSchedule();

        public void SetRotationClass();

        #endregion
    }
}
