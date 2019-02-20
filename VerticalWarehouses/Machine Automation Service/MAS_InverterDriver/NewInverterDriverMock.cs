using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public class NewInverterDriverMock : INewInverterDriver
    {
        #region Properties

        public Single GetDrawerWeight { get; set; }

        #endregion

        #region Methods

        public void Destroy()
        {
            Console.WriteLine("InverterDriverMock Destroy\n");
        }

        public void ExecuteDrawerWeight(Int32 targetPosition, Single vMax, Single acc, Single dec)
        {
            Console.WriteLine("InverterDriverMock ExecuteDrawerWeight\n");
        }

        public void ExecuteHomingStop()
        {
            Console.WriteLine("InverterDriverMock ExecuteHomingStop\n");
        }

        public void ExecuteHorizontalHoming()
        {
            Console.WriteLine("InverterDriverMock ExecuteHorizontalHoming\n");
        }

        public void ExecuteHorizontalPosition(Int32 target, Int32 speed, Int32 direction, List<ProfilePosition> profile)
        {
            Console.WriteLine("InverterDriverMock ExecuteHorizontalPosition\n");
        }

        public void ExecuteVerticalHoming()
        {
            Console.WriteLine("InverterDriverMock ExecuteVerticalHoming\n");
        }

        public void ExecuteVerticalPosition(Int32 targetPosition, Single vMax, Single acc, Single dec, Single weight, Int16 offset)
        {
            Console.WriteLine("InverterDriverMock ExecuteVerticalPosition\n");
        }

        public Boolean[] GetSensorsStates()
        {
            Console.WriteLine("InverterDriverMock ExecuteHomingStop\n");
            return new Boolean[] { true, true, true };
        }

        #endregion
    }
}
