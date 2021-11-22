using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class LogoutSettings : DataModel
    {
        #region Properties

        /// <summary>
        /// Logout timeout in minutes
        /// </summary>
        public int Timeout { get; set; }

        public TimeSpan BeginTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; }

        public double RemainingTime { get; set; }

        #endregion

        public void Validate()
        {
        }
    }
}
