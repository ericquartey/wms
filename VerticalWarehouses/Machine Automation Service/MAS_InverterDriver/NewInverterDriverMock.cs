using System;
using System.Collections.Generic;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS_InverterDriver
{
    public class NewInverterDriverMock : INewInverterDriver
    {
        #region Fields

        private readonly ILogger<NewInverterDriverMock> logger;

        #endregion

        #region Constructors

        public NewInverterDriverMock(ILogger<NewInverterDriverMock> logger)
        {
            this.logger = logger;
        }

        #endregion

        #region Properties

        public Single GetDrawerWeight { get; set; }

        #endregion

        #region Methods

        public void Destroy()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock Destroy", null);
        }

        public void ExecuteDrawerWeight(Int32 targetPosition, Single vMax, Single acc, Single dec)
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteDrawerWeight", null);
        }

        public void ExecuteHomingStop()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteHomingStop", null);
        }

        public void ExecuteHorizontalHoming()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteHorizontalHoming", null);
        }

        public void ExecuteHorizontalPosition(Int32 target, Int32 speed, Int32 direction, List<ProfilePosition> profile, Single weight)
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteHorizontalPosition", null);
        }

        public void ExecuteHorizontalPositionStop()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteHorizontalPositionStop", null);
        }

        public void ExecuteVerticalHoming()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteVerticalHoming", null);
        }

        public void ExecuteVerticalPosition(Int32 targetPosition, Single vMax, Single acc, Single dec, Single weight, Int16 offset, bool absoluteMovement)
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteVerticalPosition", null);
        }

        public void ExecuteVerticalPositionStop()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteVerticalPositionStop", null);
        }

        public Boolean[] GetSensorsStates()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock GetSensorsStates", null);
            return new Boolean[] { true, true, true };
        }

        #endregion
    }
}
