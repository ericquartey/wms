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

        public float GetDrawerWeight { get; set; }

        #endregion

        #region Methods

        public void Destroy()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock Destroy", null);
        }

        public void ExecuteDrawerWeight(int targetPosition, float vMax, float acc, float dec)
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

        public void ExecuteHorizontalPosition(int target, int speed, int direction, List<ProfilePosition> profile, float weight)
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

        public void ExecuteVerticalPosition(int targetPosition, float vMax, float acc, float dec, float weight,
            short offset, bool absoluteMovement)
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteVerticalPosition", null);
        }

        public void ExecuteVerticalPositionStop()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock ExecuteVerticalPositionStop", null);
        }

        public bool[] GetSensorsStates()
        {
            this.logger.Log(LogLevel.Debug, "InverterDriverMock GetSensorsStates", null);
            return new[] {true, true, true};
        }

        #endregion
    }
}
