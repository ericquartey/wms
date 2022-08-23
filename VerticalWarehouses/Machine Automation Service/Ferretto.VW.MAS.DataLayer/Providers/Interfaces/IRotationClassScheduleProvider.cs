using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IRotationClassScheduleProvider
    {
        #region Methods

        void AddOrModifyRotationClassSchedule(RotationClassSchedule rotationClassSchedule);

        bool CheckRotationClass();

        public void SetRotationClass();

        #endregion
    }
}
